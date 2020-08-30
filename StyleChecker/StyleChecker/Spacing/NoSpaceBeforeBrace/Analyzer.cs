namespace StyleChecker.Spacing.NoSpaceBeforeBrace
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using R = Resources;

    /// <summary>
    /// NoSpaceBeforeBrace analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : AbstractAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "NoSpaceBeforeBrace";

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

        private static void SyntaxTreeAction(
            SyntaxTreeAnalysisContext context)
        {
            static bool EndsWithSpace(SyntaxTriviaList list)
            {
                return list.Last()
                    .IsKindOneOf(
                        SyntaxKind.WhitespaceTrivia,
                        SyntaxKind.EndOfLineTrivia);
            }

            static bool IsOnFirstColumn(SyntaxToken token)
            {
                return token.GetLocation()
                        .GetLineSpan()
                        .StartLinePosition
                        .Character is 0;
            }

            static bool DoesBraceNeedSpace(
                SyntaxToken token, Func<SyntaxToken, bool> otherwisePrev)
            {
                return !IsOnFirstColumn(token)
                    && token.HasLeadingTrivia
                        ? !EndsWithSpace(token.LeadingTrivia)
                        : token.GetPreviousToken() is {} prev
                            && prev.HasTrailingTrivia
                        ? !EndsWithSpace(prev.TrailingTrivia)
                        : otherwisePrev(prev);
            }

            static bool DoesOpenBraceNeedSpace(SyntaxToken token)
            {
                return DoesBraceNeedSpace(
                    token,
                    prev => !prev.IsKindOneOf(
                        SyntaxKind.None,
                        SyntaxKind.OpenParenToken));
            }

            static bool DoesCloseBraceNeedSpace(SyntaxToken token)
            {
                return DoesBraceNeedSpace(
                    token,
                    prev => !prev.IsKindOneOf(
                        SyntaxKind.None,
                        SyntaxKind.OpenBraceToken));
            }

            var root = context.Tree
                .GetCompilationUnitRoot(context.CancellationToken);
            var leftBraces = root.DescendantTokens()
                .Where(t => t.IsKind(SyntaxKind.OpenBraceToken)
                    && !t.IsMissing
                    && DoesOpenBraceNeedSpace(t));
            var rightBraces = root.DescendantTokens()
                .Where(t => t.IsKind(SyntaxKind.CloseBraceToken)
                    && !t.IsMissing
                    && DoesCloseBraceNeedSpace(t));
            var all = leftBraces.Concat(rightBraces);
            foreach (var t in all)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, t.GetLocation(), t.Text));
            }
        }
    }
}
