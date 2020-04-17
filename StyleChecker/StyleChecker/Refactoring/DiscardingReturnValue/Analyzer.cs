namespace StyleChecker.Refactoring.DiscardingReturnValue
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Operations;
    using StyleChecker.Annotations;
    using StyleChecker.Settings;
    using GlobalNamespaceStyle
        = Microsoft.CodeAnalysis.SymbolDisplayGlobalNamespaceStyle;
    using MemberOptions
        = Microsoft.CodeAnalysis.SymbolDisplayMemberOptions;
    using ParameterOptions
        = Microsoft.CodeAnalysis.SymbolDisplayParameterOptions;
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
        /// (including its signature) and returns whether the return value of
        /// it is ignorable or not; <c>true</c> if it is not ignorable,
        /// <c>false</c> otherwise.
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
            SupportedDiagnostics => ImmutableArray.Create(Rule);

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
                    new[] { Environment.NewLine },
                    StringSplitOptions.RemoveEmptyEntries)
                .ToImmutableHashSet();
            var typePredicates
                = new Dictionary<string, Func<IMethodSymbol, bool>>
            {
                ["System.Type"] = m => !(m.Name is "InvokeMember"),
            };
            return m =>
            {
                if (methodNames.Contains(m.ToString()))
                {
                    return true;
                }
                var containingType = m.ContainingType.OriginalDefinition;
                if (containingType is null)
                {
                    return false;
                }
                var type = containingType.ToString();
                if (typeNames.Contains(type))
                {
                    return true;
                }
                var containingNamespace = m.ContainingNamespace;

                return typePredicates.TryGetValue(
                    $"{containingNamespace.Name}.{containingType.Name}",
                    out var predicate) && predicate(m);
            };
        }

        private static void AnalyzeModel(
            SemanticModelAnalysisContext context, ConfigPod pod)
        {
            var config = pod.RootConfig.DiscardingReturnValue;
            var methodSet = config.GetMethodSignatures().ToImmutableHashSet();

            var cancellationToken = context.CancellationToken;
            var model = context.SemanticModel;
            var root = model.SyntaxTree
                .GetCompilationUnitRoot(cancellationToken);
            var all = root.DescendantNodes()
                .OfType<ExpressionStatementSyntax>()
                .Select(s => s.Expression)
                .OfType<InvocationExpressionSyntax>();
            if (!all.Any())
            {
                return;
            }

            static bool IsMarkedAsDoNotIgnore(IMethodSymbol s)
                => s.GetReturnTypeAttributes()
                    .Select(d => d.AttributeClass)
                    .OfType<INamedTypeSymbol>()
                    .Select(s => s.ToString())
                    .Any(n => n == DoNotIgnoreClassName);

            bool ContainsSet(IMethodSymbol s)
            {
                var d = s.OriginalDefinition;
                return !(d is null)
                    && methodSet.Contains(d.ToDisplayString(SignatureFormat));
            }

            foreach (var invocationExpr in all)
            {
                if (!(model.GetOperation(invocationExpr)
                    is IInvocationOperation invocationOperation))
                {
                    continue;
                }
                var target = invocationOperation.TargetMethod;
                if (target.ReturnsVoid)
                {
                    continue;
                }
                if (!IsMarkedAsDoNotIgnore(target)
                    && !TargetMethodPredicate(target)
                    && !ContainsSet(target))
                {
                    continue;
                }
                var parent = invocationExpr.Parent;
                if (parent is null)
                {
                    continue;
                }
                var location = parent.GetLocation();
                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    target.OriginalDefinition.ToString());
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void StartAction(
            CompilationStartAnalysisContext context, ConfigPod pod)
        {
            context.RegisterSemanticModelAction(
                c => AnalyzeModel(c, pod));
        }
    }
}
