namespace StyleChecker.Refactoring.IsNull
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Text;
    using R = Resources;

    /// <summary>
    /// IsNull CodeFix provider (with Equality Operators).
    /// </summary>
    [ExportCodeFixProvider(
        LanguageNames.CSharp, Name = nameof(EqualNullCodeFixer))]
    [Shared]
    public sealed class EqualNullCodeFixer : AbstractCodeFixer
    {
        private static readonly ImmutableList<ReviserKit> KitList
            = ImmutableList.Create(
                new ReviserKit(
                    nameof(R.FixTitleEqualNull),
                    ReplaceIsNullWithEqualOperator),
                new ReviserKit(
                    nameof(R.FixTitleNotEqualNull),
                    ReplaceNotIsNullWithNotEqualOperator));

        /// <inheritdoc/>
        protected override ImmutableList<ReviserKit> ReviserKitList => KitList;

        private static Reviser? ReplaceIsNullWithEqualOperator(
            SyntaxNode root, TextSpan span)
        {
            var node = root.FindNodeOfType<IsPatternExpressionSyntax>(span);
            if (node is null)
            {
                return null;
            }
            var newNode = SyntaxFactory.BinaryExpression(
                    SyntaxKind.EqualsExpression,
                    node.Expression,
                    SyntaxFactory.Token(SyntaxKind.EqualsEqualsToken)
                        .WithTriviaFrom(node.IsKeyword),
                    SyntaxFactory.LiteralExpression(
                            SyntaxKind.NullLiteralExpression))
                        .WithTriviaFrom(node.Pattern)
                .WithTriviaFrom(node);
            return new Reviser(root, node, newNode);
        }

        private static Reviser? ReplaceNotIsNullWithNotEqualOperator(
            SyntaxNode root, TextSpan span)
        {
            if (!(FindNotIsNull(root, span) is {} result))
            {
                return null;
            }
            var (node, paren, isPattern) = result;

            /*
                ! T0 ( T1 value T2 is T3 null T4 ) T5
                => TO T1 value T2 != T3 null T4 T5
            */
            var newLeading = /*T0*/node.OperatorToken.GetAllTrivia()
                .Concat(/*T1*/paren.OpenParenToken.GetAllTrivia());
            var newTrailing = /*T4*/isPattern.GetTrailingTrivia()
                .Concat(/*T5*/paren.CloseParenToken.GetAllTrivia());
            var newNode = SyntaxFactory.BinaryExpression(
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
            return new Reviser(root, node, newNode);
        }
    }
}
