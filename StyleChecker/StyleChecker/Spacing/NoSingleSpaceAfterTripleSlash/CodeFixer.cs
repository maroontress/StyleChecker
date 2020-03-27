namespace StyleChecker.Spacing.NoSingleSpaceAfterTripleSlash
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using R = Resources;

    /// <summary>
    /// NoSingleSpaceAfterTripleSlash code fix provider.
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
            var insertFixTitle = localize(nameof(R.InsertFixTitle))
                .ToString(CultureInfo.CurrentCulture);
            var replaceFixTitle = localize(nameof(R.ReplaceFixTitle))
                .ToString(CultureInfo.CurrentCulture);

            var root = await context
                .Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics[0];
            var span = diagnostic.Location.SourceSpan;
            var trivia = root.FindTrivia(span.Start, findInsideTrivia: true);
            if (trivia == default)
            {
                return;
            }

            static string Unify(string t) => $" {t.TrimStart()}";

            static string Prepend(string t) => $" {t}";

            static CodeAction NewAction(
                string fixTitle,
                Func<CancellationToken, Task<Document>> createChangedDocument)
                => CodeAction.Create(
                    title: fixTitle,
                    createChangedDocument: createChangedDocument,
                    equivalenceKey: fixTitle);

            var document = context.Document;
            var token = trivia.Token;
            var node = token.Parent;

            CodeAction ReplaceTokenAction(
                string fixTitle, Func<string, string> replacer)
                => NewAction(
                    fixTitle,
                    c => ReplaceTask(document, root, token, replacer));

            CodeAction InsertTriviaAction()
                => NewAction(
                    insertFixTitle,
                    c => InsertTriviaTask(document, root, token));

            CodeAction ReplaceTriviaAction(SyntaxTrivia trivia)
                => NewAction(
                    replaceFixTitle,
                    c => ReplaceTriviaTask(document, root, token, trivia));

            CodeAction? GetAction()
            {
                if (token.Kind() is SyntaxKind.XmlTextLiteralToken
                    && node is XmlTextSyntax)
                {
                    var (fixTitle, replacer)
                        = (node.Parent is DocumentationCommentTriviaSyntax)
                            ? (replaceFixTitle, (Func<string, string>)Unify)
                            : (insertFixTitle, Prepend);

                    return ReplaceTokenAction(fixTitle, replacer);
                }
                var trivias = token.LeadingTrivia;
                var index = trivias.IndexOf(trivia);
                if (trivias.Count == index + 1)
                {
                    return InsertTriviaAction();
                }
                var nextTrivia = trivias[index + 1];
                return nextTrivia.Kind() is SyntaxKind.WhitespaceTrivia
                    ? ReplaceTriviaAction(nextTrivia)
                    : null;
            }

            var action = GetAction();
            if (action is null)
            {
                return;
            }
            context.RegisterCodeFix(action, diagnostic);
        }

        private async Task<Document> ReplaceTask(
            Document document,
            SyntaxNode root,
            SyntaxToken token,
            Func<string, string> toNewText)
        {
            var text = token.Text;
            var newText = toNewText(text);
            var newToken = SyntaxFactory.Token(
                token.LeadingTrivia,
                SyntaxKind.XmlTextLiteralToken,
                newText,
                newText,
                token.TrailingTrivia);
            var newRoot = root.ReplaceToken(token, newToken);
            return await Task.Run(() => document.WithSyntaxRoot(newRoot))
                .ConfigureAwait(false);
        }

        private async Task<Document> InsertTriviaTask(
            Document document,
            SyntaxNode root,
            SyntaxToken token)
        {
            var trivias = token.LeadingTrivia;
            var newTrivias = trivias.Add(
                SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "));
            var newToken = token.WithLeadingTrivia(newTrivias);
            var newRoot = root.ReplaceToken(token, newToken);
            return await Task.Run(() => document.WithSyntaxRoot(newRoot))
                .ConfigureAwait(false);
        }

        private async Task<Document> ReplaceTriviaTask(
            Document document,
            SyntaxNode root,
            SyntaxToken token,
            SyntaxTrivia whiteSpaceTrivia)
        {
            var trivias = token.LeadingTrivia;
            var newWhiteSpaceTrivia = SyntaxFactory.SyntaxTrivia(
                SyntaxKind.WhitespaceTrivia, " ");
            var newTrivias = trivias.Replace(
                whiteSpaceTrivia, newWhiteSpaceTrivia);
            var newToken = token.WithLeadingTrivia(newTrivias);
            var newRoot = root.ReplaceToken(token, newToken);
            return await Task.Run(() => document.WithSyntaxRoot(newRoot))
                .ConfigureAwait(false);
        }
    }
}
