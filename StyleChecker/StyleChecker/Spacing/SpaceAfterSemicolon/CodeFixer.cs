namespace StyleChecker.Spacing.SpaceAfterSemicolon
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using R = Resources;

    /// <summary>
    /// SpaceAfterSemicolon code fix provider.
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
            var localize = Localizers.Of(R.ResourceManager, typeof(R));
            var title = localize(nameof(R.FixTitle)).ToString();

            var root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument:
                        c => FixTask(
                            context.Document, diagnostic, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> FixTask(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document
                .GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            var span = diagnostic.Location.SourceSpan;
            var token = root.FindToken(span.Start, findInsideTrivia: true);
            var node = root.FindNode(span);

            var list = token.TrailingTrivia;
            var space = SyntaxFactory.ParseTrailingTrivia(" ");
            var newList = list.InsertRange(0, space);
            var newToken = token.WithTrailingTrivia(newList);
            root = root.ReplaceToken(token, newToken);
            return document.WithSyntaxRoot(root);
        }
    }
}
