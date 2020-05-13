namespace StyleChecker.Document.StrayText
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection.Metadata.Ecma335;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using R = Resources;

    /// <summary>
    /// StrayText analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : AbstractAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = nameof(StrayText);

        private const string Category = Categories.Document;
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

        private static void SyntaxTreeAction(SyntaxTreeAnalysisContext context)
        {
            static IEnumerable<SyntaxToken> ToStrayText(XmlTextSyntax n)
            {
                if (!(n.Parent is DocumentationCommentTriviaSyntax))
                {
                    return Enumerable.Empty<SyntaxToken>();
                }
                return n.TextTokens
                    .Where(t => t.Kind() == SyntaxKind.XmlTextLiteralToken
                        && t.Text.Trim().Length > 0)
                    .Take(1);
            }

            var root = context.Tree.GetCompilationUnitRoot(
                context.CancellationToken);
            var all = root.DescendantNodes(descendIntoTrivia: true)
                .OfType<XmlTextSyntax>()
                .SelectMany(ToStrayText);
            foreach (var t in all)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Rule, t.GetLocation(), t.ToString().Trim()));
            }
        }
    }
}
