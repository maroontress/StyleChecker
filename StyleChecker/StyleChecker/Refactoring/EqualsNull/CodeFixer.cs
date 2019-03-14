namespace StyleChecker.Refactoring.EqualsNull
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
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
    /// EqualsNull CodeFix provider.
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
            string FixTitle(string key)
            {
                var localize = Localizers.Of<R>(R.ResourceManager);
                return localize(key).ToString(CultureInfo.CurrentCulture);
            }

            var root = await context
                .Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var span = diagnostic.Location.SourceSpan;

            var node = root.FindNodeOfType<BinaryExpressionSyntax>(span);
            if (node == null)
            {
                return;
            }

            void Register(SyntaxKind kind, string key)
            {
                if (node.OperatorToken.Kind() != kind)
                {
                    return;
                }
                var title = FixTitle(key);
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedSolution:
                            c => Replace(context.Document, node, c),
                        equivalenceKey: title),
                    diagnostic);
            }
            Register(
                SyntaxKind.ExclamationEqualsToken,
                nameof(R.FixTitleIsNotNull));
            Register(
                SyntaxKind.EqualsEqualsToken,
                nameof(R.FixTitleIsNull));
        }

        private static async Task<Solution> Replace(
            Document document,
            BinaryExpressionSyntax node,
            CancellationToken cancellationToken)
        {
            var solution = document.Project.Solution;
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);

            var newNode = SyntaxFactory.IsPatternExpression(
                node.Left,
                SyntaxFactory.Token(SyntaxKind.IsKeyword)
                    .WithTriviaFrom(node.OperatorToken),
                SyntaxFactory.ConstantPattern(
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.NullLiteralExpression))
                    .WithTriviaFrom(node.Right)) as ExpressionSyntax;

            if (node.OperatorToken.Kind()
                == SyntaxKind.ExclamationEqualsToken)
            {
                newNode = SyntaxFactory.PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    SyntaxFactory.ParenthesizedExpression(newNode));
            }

            newNode = newNode.WithTriviaFrom(node);

            var newRoot = root.ReplaceNode(node, newNode);
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
