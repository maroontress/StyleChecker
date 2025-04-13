namespace StyleChecker.Analyzers.Spacing.NoSpaceAfterSemicolon;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using StyleChecker.Analyzers;
using R = Resources;

/// <summary>
/// NoSpaceAfterSemicolon analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "NoSpaceAfterSemicolon";

    private const string Category = Categories.Spacing;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(SyntaxTreeAction);
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

    private static bool IsSpaceNeeded(SyntaxToken token)
    {
        var parent = token.Parent;
        var next = token.GetNextToken();
        return !next.IsKind(SyntaxKind.None)
            && (!parent.IsKind(SyntaxKind.ForStatement)
                || (!next.IsKind(SyntaxKind.CloseParenToken)
                    && !next.IsKind(SyntaxKind.SemicolonToken)))
            && (!token.HasTrailingTrivia
                /* token.TrailingTrivia.First() is safe. */
                || !token.TrailingTrivia.First().IsKindOneOf(
                    SyntaxKind.WhitespaceTrivia,
                    SyntaxKind.EndOfLineTrivia));
    }

    private static void SyntaxTreeAction(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree
            .GetCompilationUnitRoot(context.CancellationToken);
        var all = root.DescendantTokens()
            .Where(t => t.IsKind(SyntaxKind.SemicolonToken)
                && !t.IsMissing
                && IsSpaceNeeded(t))
            .Select(t => Diagnostic.Create(Rule, t.GetLocation(), t.Text))
            .ToList();
        foreach (var d in all)
        {
            context.ReportDiagnostic(d);
        }
    }
}
