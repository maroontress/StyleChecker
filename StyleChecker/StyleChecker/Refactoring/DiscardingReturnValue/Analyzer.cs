namespace StyleChecker.Refactoring.DiscardingReturnValue;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Maroontress.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using StyleChecker.Annotations;
using StyleChecker.Settings;
using GlobalNamespaceStyle
    = Microsoft.CodeAnalysis.SymbolDisplayGlobalNamespaceStyle;
using MemberOptions = Microsoft.CodeAnalysis.SymbolDisplayMemberOptions;
using ParameterOptions = Microsoft.CodeAnalysis.SymbolDisplayParameterOptions;
using R = Resources;

/// <summary>
/// DiscardingReturnValue analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "DiscardingReturnValue";

    private const string Category = Categories.Refactoring;

    /// <summary>
    /// The function that takes the fully qualified name of the method
    /// (including its signature) and returns whether the return value of it is
    /// ignorable or not; <c>true</c> if it is not ignorable, <c>false</c>
    /// otherwise.
    /// </summary>
#pragma warning disable RS1008
    private static readonly Func<IMethodSymbol, bool>
        TargetMethodPredicate = NewTargetMethodPredicate();
#pragma warning restore RS1008

    private static readonly DiagnosticDescriptor Rule = NewRule();

    private static readonly string DoNotIgnoreClassName
        = typeof(DoNotIgnoreAttribute).FullName;

    private static readonly SymbolDisplayFormat SignatureFormat
        = SymbolDisplayFormat.FullyQualifiedFormat
            .WithMemberOptions(
                MemberOptions.IncludeParameters
                | MemberOptions.IncludeContainingType)
            .WithGlobalNamespaceStyle(GlobalNamespaceStyle.Omitted)
            .WithParameterOptions(ParameterOptions.IncludeType);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        ConfigBank.LoadRootConfig(context, StartAction);
        context.EnableConcurrentExecution();
    }

    private static DiagnosticDescriptor NewRule()
    {
        var localize = Localizers.Of<R>(R.ResourceManager);
        return new DiagnosticDescriptor(
            DiagnosticId,
            localize(nameof(R.Title)),
            localize(nameof(R.MessageFormat)),
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: localize(nameof(R.Description)),
            helpLinkUri: HelpLink.ToUri(DiagnosticId));
    }

    private static Func<IMethodSymbol, bool> NewTargetMethodPredicate()
    {
        var methodNames = new HashSet<string>()
        {
            "System.IO.Stream.Read(byte[], int, int)",
            "System.IO.BinaryReader.Read(byte[], int, int)",
        };

        static string GetTypeNames() => EmbeddedResources.GetText(
            "Refactoring.DiscardingReturnValue", "TypeNames.txt");

        var typeNames = GetTypeNames().Split(
                [Platforms.NewLine()], StringSplitOptions.RemoveEmptyEntries)
            .ToImmutableHashSet();
        var typePredicates = new Dictionary<string, Func<IMethodSymbol, bool>>
        {
            ["System.Type"] = m => m.Name is not "InvokeMember",
        };
        return m =>
        {
            if (methodNames.Contains(m.ToString()))
            {
                return true;
            }
            if (m.ContainingType.OriginalDefinition is not {} containingType)
            {
                return false;
            }
            if (typeNames.Contains(containingType.ToString()))
            {
                return true;
            }
            return typePredicates.TryGetValue(
                $"{m.ContainingNamespace.Name}.{containingType.Name}",
                out var predicate) && predicate(m);
        };
    }

    private static void AnalyzeModel(
        SemanticModelAnalysisContext context, ConfigPod pod)
    {
        static bool IsMarkedAsDoNotIgnore(IMethodSymbol s)
            => s.GetReturnTypeAttributes()
                .Select(d => d.AttributeClass)
                .FilterNonNullReference()
                .Select(s => s.ToString())
                .Any(n => n == DoNotIgnoreClassName);

        static Func<IMethodSymbol, bool> NewContainsSet(
            IReadOnlyCollection<string> set)
        {
            return s => s.OriginalDefinition is {} d
                && set.Contains(d.ToDisplayString(SignatureFormat));
        }

        var config = pod.RootConfig.DiscardingReturnValue;
        var methodSet = config.GetMethodSignatures()
            .ToImmutableHashSet();
        var containsSet = NewContainsSet(methodSet);

        var cancellationToken = context.CancellationToken;
        var model = context.SemanticModel;
        var root = model.SyntaxTree
            .GetCompilationUnitRoot(cancellationToken);
        var all = root.DescendantNodes()
            .OfType<ExpressionStatementSyntax>()
            .Select(s => s.Expression)
            .OfType<InvocationExpressionSyntax>()
            .SelectMany(s => (model.GetOperation(s) is IInvocationOperation o
                    && o.TargetMethod is {ReturnsVoid: false} target
                    && (IsMarkedAsDoNotIgnore(target)
                        || TargetMethodPredicate(target)
                        || containsSet(target))
                    && s.Parent is {} parent)
                ? [(parent, target)]
                : Enumerable.Empty<(SyntaxNode, IMethodSymbol)>())
            .ToList();

        foreach (var (parent, target) in all)
        {
            var location = parent.GetLocation();
            var diagnostic = Diagnostic.Create(
                Rule, location, target.OriginalDefinition.ToString());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private void StartAction(
        CompilationStartAnalysisContext context, ConfigPod pod)
    {
        context.RegisterSemanticModelAction(c => AnalyzeModel(c, pod));
    }
}
