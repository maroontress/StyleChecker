namespace StyleChecker.Spacing.NoSpaceAfterSemicolon
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using R = Resources;

    /// <summary>
    /// NoSpaceAfterSemicolon analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "NoSpaceAfterSemicolon";

        private const string Category = Categories.Spacing;
        private static readonly DiagnosticDescriptor Rule = NewRule();

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);
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
            var next = token.GetNextToken();
            if (next.IsKind(SyntaxKind.None))
            {
                return false;
            }
            var parent = token.Parent;
            if (parent.IsKind(SyntaxKind.ForStatement)
                && (next.IsKind(SyntaxKind.CloseParenToken)
                    || next.IsKind(SyntaxKind.SemicolonToken)))
            {
               return false;
            }
            return !token.HasTrailingTrivia
                /* token.TrailingTrivia.First() is safe. */
                || !token.TrailingTrivia.First().IsKindOneOf(
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
                    && IsSpaceNeeded(t))
                .ToArray();
            foreach (var t in all)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, t.GetLocation(), t.Text));
            }
        }
    }
}
