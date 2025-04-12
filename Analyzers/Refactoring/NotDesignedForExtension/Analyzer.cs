namespace Analyzers.Refactoring.NotDesignedForExtension;

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roastery;
using R = Resources;

/// <summary>
/// NotDesignedForExtension analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "NotDesignedForExtension";

    private const string Category = Categories.Refactoring;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.RegisterSemanticModelAction(AnalyzeModel);
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

    private static T? ToNode<T>(ISymbol m)
            where T : SyntaxNode
        => (m.DeclaringSyntaxReferences
            .FirstOrDefault() is not {} reference)
            ? null
            : reference.GetSyntax() as T;

    private static SyntaxToken? ToToken<T>(
            ISymbol symbol, Func<T, SyntaxToken> map)
        where T : SyntaxNode
        => (ToNode<T>(symbol) is not {} node)
            ? null
            : map(node);

    private static SyntaxToken? ToToken(IMethodSymbol m)
        => ToToken<MethodDeclarationSyntax>(m, s => s.Identifier);

    private static SyntaxToken? ToToken(IPropertySymbol m)
        => ToToken<PropertyDeclarationSyntax>(m, s => s.Identifier);

    private static bool HasNoBlock(MethodDeclarationSyntax n)
        => n.Body is null
            && n.ExpressionBody is null;

    private static bool HasAnEmptyBlock(MethodDeclarationSyntax n)
        => n.Body is BlockSyntax block
            && !block.Statements.Any();

    private static bool IsEmpty(IMethodSymbol m)
        => ToNode<MethodDeclarationSyntax>(m) is {} node
            && (HasNoBlock(node) || HasAnEmptyBlock(node));

    private void AnalyzeModel(SemanticModelAnalysisContext context)
    {
        var cancellationToken = context.CancellationToken;
        var model = context.SemanticModel;
        var root = model.SyntaxTree
            .GetCompilationUnitRoot(cancellationToken);
        var allMembers = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Select(n => model.GetDeclaredSymbol(n, cancellationToken))
            .FilterNonNullReference()
            .Where(s => !s.IsSealed && !s.IsStatic)
            .SelectMany(s => s.GetMembers());

        var allMethods = allMembers.OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary
                && ((m.IsVirtual && (!m.ReturnsVoid || !IsEmpty(m)))
                    || (m.IsOverride && !m.IsSealed)))
            .Select(m => ToToken(m))
            .FilterNonNullValue()
            .Select(t => (t, R.Method));
        var allProperties = allMembers.OfType<IPropertySymbol>()
            .Where(p => p.IsVirtual || (p.IsOverride && !p.IsSealed))
            .Select(p => ToToken(p))
            .FilterNonNullValue()
            .Select(t => (t, R.Property));
        var all = allMethods.Concat(allProperties)
            .ToList();
        foreach (var (token, format) in all)
        {
            var location = token.GetLocation();
            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                string.Format(CompilerCulture, format, token));
            context.ReportDiagnostic(diagnostic);
        }
    }
}
