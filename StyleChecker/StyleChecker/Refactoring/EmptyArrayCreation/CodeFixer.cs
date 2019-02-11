namespace StyleChecker.Refactoring.EmptyArrayCreation
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
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;
    using R = Resources;

    /// <summary>
    /// EmptyArrayCreation CodeFix provider.
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

            var root = await context
                .Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution:
                        c => Replace(context.Document, node, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private static async Task<Solution> Replace(
            Document document,
            SyntaxNode node,
            CancellationToken cancellationToken)
        {
            var solution = document.Project.Solution;
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            if (!(node is ArrayCreationExpressionSyntax arrayCreationNode))
            {
                return solution;
            }
            var type = arrayCreationNode.Type.ElementType;
            var newArrayCreationNode = SyntaxFactory.ParseExpression(
                $"System.Array.Empty<{type}>()");
            var newRoot = root.ReplaceNode(
                arrayCreationNode, newArrayCreationNode);
            if (newRoot == null)
            {
                return solution;
            }
            var workspace = solution.Workspace;
            var formattedNode = Formatter.Format(
               newRoot,
               Formatter.Annotation,
               workspace,
               workspace.Options);
            return solution.WithDocumentSyntaxRoot(
                document.Id, formattedNode);
        }
    }
}
