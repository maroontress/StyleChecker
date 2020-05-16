namespace StyleChecker.Refactoring.StinkyBooleanExpression
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
    using Microsoft.CodeAnalysis.Formatting;
    using R = Resources;

    /// <summary>
    /// StinkyBooleanExpression CodeFix provider.
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
            static string FixTitle(string key)
            {
                var localize = Localizers.Of<R>(R.ResourceManager);
                return localize(key).ToString(CultureInfo.CurrentCulture);
            }

            var root = await context
                .Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);
            if (root is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics[0];
            var span = diagnostic.Location.SourceSpan;

            void Register(
                Func<ConditionalExpressionSyntax, SyntaxNode> replacer,
                SyntaxKind kind,
                string key)
            {
                var node = root
                    .FindNodeOfType<ConditionalExpressionSyntax>(span);
                if (node is null)
                {
                    return;
                }
                if (node.WhenTrue.Kind() != kind
                    && node.WhenFalse.Kind() != kind)
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

            Register(
                ReplaceConditionWithLogicalAnd,
                SyntaxKind.FalseLiteralExpression,
                nameof(R.FixTitleUseConditionalLogicalAnd));
            Register(
                ReplaceConditionWithLogicalOr,
                SyntaxKind.TrueLiteralExpression,
                nameof(R.FixTitleUseConditionalLogicalOr));
        }

        private static ParenthesizedExpressionSyntax Parenthesize(
            ExpressionSyntax s)
        {
            return s is ParenthesizedExpressionSyntax parenthesized
                ? parenthesized
                : SyntaxFactory.ParenthesizedExpression(s);
        }

        private static PrefixUnaryExpressionSyntax? GetLogicalNot(
            ExpressionSyntax s)
        {
            return (s.Kind() is SyntaxKind.LogicalNotExpression
                   && s is PrefixUnaryExpressionSyntax logicalNot)
                ? logicalNot
                : null;
        }

        private static ExpressionSyntax Negate(ExpressionSyntax node)
        {
            static ExpressionSyntax Reverse(ExpressionSyntax s)
            {
                return (s.Kind() is SyntaxKind.LogicalNotExpression
                        && s is PrefixUnaryExpressionSyntax logicalNot)
                    /* !... => (...) */
                    ? (ExpressionSyntax)Parenthesize(logicalNot.Operand)
                        .WithTriviaFrom(s)
                    /* ... => !(...) */
                    : SyntaxFactory.PrefixUnaryExpression(
                        SyntaxKind.LogicalNotExpression,
                        Parenthesize(s));
            }

            return (node is ParenthesizedExpressionSyntax parenthesized
                    && GetLogicalNot(parenthesized.Expression)
                        is PrefixUnaryExpressionSyntax logicalNot)
                /* (!...) => (...) */
                ? Parenthesize(logicalNot.Operand)
                    .WithTriviaFrom(node)
                : Reverse(node);
        }

        private static SyntaxNode ReplaceConditionWithLogicalAnd(
            ConditionalExpressionSyntax node)
        {
            var logicalAndToken = SyntaxFactory
                .Token(SyntaxKind.AmpersandAmpersandToken);
            return ReplaceConditionWith(
                node,
                SyntaxKind.FalseLiteralExpression,
                Parenthesize,
                Negate,
                logicalAndToken,
                SyntaxKind.LogicalAndExpression);
        }

        private static SyntaxNode ReplaceConditionWithLogicalOr(
            ConditionalExpressionSyntax node)
        {
            var logicalOrToken = SyntaxFactory
                .Token(SyntaxKind.BarBarToken);
            return ReplaceConditionWith(
                node,
                SyntaxKind.TrueLiteralExpression,
                Negate,
                Parenthesize,
                logicalOrToken,
                SyntaxKind.LogicalOrExpression);
        }

        private static SyntaxNode ReplaceConditionWith(
            ConditionalExpressionSyntax node,
            SyntaxKind boolLiteralKind,
            Func<ExpressionSyntax, ExpressionSyntax> trueWrap,
            Func<ExpressionSyntax, ExpressionSyntax> falseWrap,
            SyntaxToken operatorToken,
            SyntaxKind binaryOperatorKind)
        {
            var (wrap, rightNode) = (node.WhenFalse.Kind() == boolLiteralKind)
                ? (trueWrap, node.WhenTrue)
                : (falseWrap, node.WhenFalse);
            var newNode = SyntaxFactory.BinaryExpression(
                    binaryOperatorKind,
                    wrap(node.Condition),
                    operatorToken.WithTriviaFrom(node.QuestionToken),
                    Parenthesize(rightNode))
                .WithTriviaFrom(node);
            return node.Parent is ParenthesizedExpressionSyntax
                /* ... => ... */
                ? (SyntaxNode)newNode
                /* ... => (...) */
                : SyntaxFactory.ParenthesizedExpression(newNode);
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
            if (root is null)
            {
                return solution;
            }

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
