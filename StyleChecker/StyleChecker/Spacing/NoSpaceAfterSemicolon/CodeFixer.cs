namespace StyleChecker.Spacing.NoSpaceAfterSemicolon
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using R = Resources;

    /// <summary>
    /// NoSpaceAfterSemicolon code fix provider.
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
            var title = localize(nameof(R.FixTitle)).ToString();

            var root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
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
            var list = token.TrailingTrivia;
            var space = SyntaxFactory.ParseTrailingTrivia(" ");
            var newList = list.InsertRange(0, space);
            var newToken = token.WithTrailingTrivia(newList);
            var newRoot = root.ReplaceToken(token, newToken);
            return await Task.Run(() => document.WithSyntaxRoot(newRoot))
                .ConfigureAwait(false);
        }
    }
}
