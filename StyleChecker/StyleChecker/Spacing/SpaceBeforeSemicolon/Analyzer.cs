namespace StyleChecker.Spacing.SpaceBeforeSemicolon
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using R = Resources;

    /// <summary>
    /// SpaceBeforeSemicolon analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : AbstractAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "SpaceBeforeSemicolon";

        private const string Category = Categories.Spacing;
        private static readonly DiagnosticDescriptor Rule = NewRule();

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        private static bool IsSpaceNotNeeded(SyntaxToken token)
        {
            if (token.HasLeadingTrivia
                && token.LeadingTrivia.Last()
                    .IsKind(SyntaxKind.WhitespaceTrivia))
            {
                return true;
            }
            var prev = token.GetPreviousToken();
            return prev.HasTrailingTrivia
                && prev.TrailingTrivia.Last().IsKindOneOf(
                    SyntaxKind.WhitespaceTrivia,
                    SyntaxKind.EndOfLineTrivia);
        }

        private static void SyntaxTreeAction(
            SyntaxTreeAnalysisContext context)
        {
            SyntaxNode root = context.Tree.GetCompilationUnitRoot(
                context.CancellationToken);
            var all = root.DescendantTokens()
                .Where(t => t.IsKind(SyntaxKind.SemicolonToken)
                    && !t.IsMissing
                    && IsSpaceNotNeeded(t))
                .ToArray();
            foreach (var t in all)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, t.GetLocation(), t.Text));
            }
        }
    }
}
