using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StyleChecker
{
    [ExportCodeFixProvider(LanguageNames.CSharp,
        Name = nameof(UnderscoreCodeFixProvider)), Shared]
    public class PostIncrementCodeFixProvider : CodeFixProvider
    {
        private const string title
            = "Replace a post-increment/decrement operator with a "
            + "pre-increment/decrement operator.";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(PostIncrementAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(
            CodeFixContext context)
        {
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
                        c => Replace(context.Document,
                            node, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> Replace(Document document,
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
                n => (n.IsKind(SyntaxKind.PlusPlusToken)
                   || n.IsKind(SyntaxKind.MinusMinusToken)));
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
