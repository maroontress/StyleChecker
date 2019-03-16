namespace StyleChecker.Refactoring.IsNull
{
    using System;
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
    /// IsNull CodeFix provider.
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

            void Register<T>(Func<T, SyntaxNode> replacer, string key)
                where T : SyntaxNode
            {
                var node = root.FindNodeOfType<T>(span);
                if (node is null)
                {
                    return;
                }
                var title = FixTitle(key);
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedSolution:
                            c => Replace(context.Document, node, replacer, c),
                        equivalenceKey: title),
                    diagnostic);
            }

            Register<IsPatternExpressionSyntax>(
                ReplaceIsWithEqual, nameof(R.FixTitleIsNull));
            Register<PrefixUnaryExpressionSyntax>(
                ReplaceIsWithNotEqual, nameof(R.FixTitleIsNotNull));
        }

        private static SyntaxNode ReplaceIsWithEqual(
            IsPatternExpressionSyntax node)
        {
            return SyntaxFactory.BinaryExpression(
                    SyntaxKind.EqualsExpression,
                    node.Expression,
                    SyntaxFactory.Token(SyntaxKind.EqualsEqualsToken)
                        .WithTriviaFrom(node.IsKeyword),
                    SyntaxFactory.LiteralExpression(
                            SyntaxKind.NullLiteralExpression))
                        .WithTriviaFrom(node.Pattern)
                .WithTriviaFrom(node);
        }

        private static SyntaxNode ReplaceIsWithNotEqual(
            PrefixUnaryExpressionSyntax node)
        {
            if (!(node.Operand is ParenthesizedExpressionSyntax paren)
                || !(paren.Expression is IsPatternExpressionSyntax isPattern))
            {
                return node;
            }
            /*
                ! T0 ( T1 value T2 is T3 null T4 ) T5
                => TO T1 value T2 != T3 null T4 T5
            */
            var newLeading = /*T0*/node.OperatorToken.GetAllTrivia()
                .Concat(/*T1*/paren.OpenParenToken.GetAllTrivia());
            var newTrailing = /*T4*/isPattern.GetTrailingTrivia()
                .Concat(/*T5*/paren.CloseParenToken.GetAllTrivia());
            return SyntaxFactory.BinaryExpression(
                    SyntaxKind.EqualsExpression,
                    /*T2*/
                    isPattern.Expression,
                    /*T3*/
                    SyntaxFactory.Token(SyntaxKind.ExclamationEqualsToken)
                        .WithTriviaFrom(isPattern.IsKeyword),
                    SyntaxFactory.LiteralExpression(
                            SyntaxKind.NullLiteralExpression))
                .WithLeadingTrivia(newLeading)
                .WithTrailingTrivia(newTrailing);
        }

        private static async Task<Solution> Replace<T>(
            Document document,
            T node,
            Func<T, SyntaxNode> getNewNode,
            CancellationToken cancellationToken)
            where T : SyntaxNode
        {
            var solution = document.Project.Solution;
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);

            var newNode = getNewNode(node);
            var newRoot = root.ReplaceNode(node, newNode);
            if (newRoot is null)
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
