namespace StyleChecker.Spacing.SpaceBeforeSemicolon
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using R = Resources;

    /// <summary>
    /// SpaceBeforeSemicolon analyzer.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixer))]
    [Shared]
    public sealed class CodeFixer : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(Analyzer.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(
            CodeFixContext context)
        {
            var localize = Localizers.Of<R>(R.ResourceManager);
            var title = localize(nameof(R.FixTitle))
                .ToString(CultureInfo.CurrentCulture);

            var root = await context
                .Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics[0];
            var span = diagnostic.Location.SourceSpan;
            var token = root.FindToken(span.Start, findInsideTrivia: true);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument:
                        c => FixTask(context.Document, root, token),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> FixTask(
            Document document,
            SyntaxNode root,
            SyntaxToken token)
        {
            SyntaxTriviaList Trim(SyntaxTriviaList triviaList)
            {
                var list = triviaList;
                var target = list.Last();
                while (target.IsKindOneOf(
                    SyntaxKind.WhitespaceTrivia,
                    SyntaxKind.EndOfLineTrivia))
                {
                    list = list.Remove(target);
                    if (!list.Any())
                    {
                        break;
                    }
                    target = list.Last();
                }
                return list;
            }

            var map = new Dictionary<SyntaxToken, SyntaxToken>();
            if (token.HasLeadingTrivia
                && token.LeadingTrivia
                    .Last()
                    .IsKind(SyntaxKind.WhitespaceTrivia))
            {
                var triviaList = Trim(token.LeadingTrivia);
                var newToken = token.WithLeadingTrivia(triviaList);
                map.Add(token, newToken);
            }
            var prev = token.GetPreviousToken();
            if (prev.HasTrailingTrivia
                && prev.TrailingTrivia.Last().IsKindOneOf(
                    SyntaxKind.WhitespaceTrivia,
                    SyntaxKind.EndOfLineTrivia))
            {
                var triviaList = Trim(prev.TrailingTrivia);
                var newPrev = prev.WithTrailingTrivia(triviaList);
                map.Add(prev, newPrev);
            }
            var keys = map.Keys;
            var newRoot = root.ReplaceTokens(keys, (k, n) => map[k]);
            return await Task.Run(() => document.WithSyntaxRoot(newRoot))
                .ConfigureAwait(false);
        }
    }
}
