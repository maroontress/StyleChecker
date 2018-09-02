namespace StyleChecker.Ordering.PostIncrement
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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixer))]
    [Shared]
    public sealed class CodeFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(Analyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(
            CodeFixContext context)
        {
            var localize = Localizers.Of(R.ResourceManager, typeof(R));
            var title = localize(nameof(R.FixTitle)).ToString();

            var root = await context
                .Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument:
                        c => Replace(context.Document, node, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> Replace(
            Document document,
            SyntaxNode node,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            var children = node.ChildNodes();
            var id = children.FirstOrDefault(
                n => n.IsKind(SyntaxKind.IdentifierName));
            var childTokens = node.ChildTokens();
            var token = childTokens.FirstOrDefault(
                n => n.IsKind(SyntaxKind.PlusPlusToken)
                   || n.IsKind(SyntaxKind.MinusMinusToken));
            var newNode = SyntaxFactory.ParseExpression(
                    token.ToString() + id.ToString())
                .WithLeadingTrivia(node.GetLeadingTrivia())
                .WithTrailingTrivia(node.GetTrailingTrivia());
            var newRoot = root.ReplaceNode(node, newNode);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
