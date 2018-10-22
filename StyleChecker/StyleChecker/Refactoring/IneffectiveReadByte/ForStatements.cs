namespace StyleChecker.Refactoring.IneffectiveReadByte
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Provides utility methods for a for statement.
    /// </summary>
    public static class ForStatements
    {
        /// <summary>
        /// Gets the loop index and range associated with the specified
        /// ForStatement node.
        /// </summary>
        /// <param name="model">
        /// The semantic model.
        /// </param>
        /// <param name="node">
        /// The ForStatement node.
        /// </param>
        /// <returns>
        /// The properties of the index and its range if the <paramref
        /// name="node"/> is a <c>for</c> statement and the range of the loop
        /// index is constant, <c>null</c> otherwise.
        /// </returns>
        public static LoopIndexRange GetLoopIndexRange(
            SemanticModel model, SyntaxNode node)
        {
            if (!(node is ForStatementSyntax forNode))
            {
                return null;
            }
            var declaration = forNode.Declaration;
            var allInitializers = forNode.Initializers;
            var condition = forNode.Condition;
            var allIncrementors = forNode.Incrementors;

            if (declaration == null
                || condition == null
                || allInitializers.Count != 0
                || allIncrementors.Count != 1)
            {
                return null;
            }
            var incrementor = allIncrementors[0];

            var context = new ForLoopIndexRangeContext();
            if (!VariableDeclarationIsConstantInitializer(
                    declaration,
                    context.First)
                || !ExpressionIsBinaryLeftIdRightNumber(
                    condition,
                    k => k == SyntaxKind.LessThanToken
                        || k == SyntaxKind.LessThanEqualsToken,
                    context.Second)
                || !ExpressionIsPreOrPostIncrement(
                    incrementor,
                    context.Third)
                || !context.IsValid())
            {
                return null;
            }
            var token = context.Id;
            var span = token.Span;
            if (!(model.LookupSymbols(span.Start, null, token.Text)
                .FirstOrDefault() is ILocalSymbol symbol))
            {
                return null;
            }
            var typeSymbol = symbol.Type;
            if (typeSymbol.SpecialType != SpecialType.System_Int32)
            {
                return null;
            }
            var dataFlow = model.AnalyzeDataFlow(forNode.Statement);
            var isWrittenInsideLoop = dataFlow.WrittenInside
                .Where(s => s.Equals(symbol))
                .Any();
            return isWrittenInsideLoop
                ? null
                : new LoopIndexRange(symbol, context.Start, context.End);
        }

        private static bool ExpressionIsPreOrPostIncrement(
            ExpressionSyntax node,
            Action<SyntaxToken> found)
        {
            SyntaxToken operatorToken;
            ExpressionSyntax operand;
            if (node is PrefixUnaryExpressionSyntax pre)
            {
                operatorToken = pre.OperatorToken;
                operand = pre.Operand;
            }
            else if (node is PostfixUnaryExpressionSyntax post)
            {
                operatorToken = post.OperatorToken;
                operand = post.Operand;
            }
            else
            {
                return false;
            }
            if (!operatorToken.IsKind(SyntaxKind.PlusPlusToken))
            {
                return false;
            }
            if (!(operand is IdentifierNameSyntax idName))
            {
                return false;
            }
            found(idName.Identifier);
            return true;
        }

        private static bool ExpressionIsBinaryLeftIdRightNumber(
            ExpressionSyntax node,
            Func<SyntaxKind, bool> judge,
            Action<SyntaxToken, SyntaxToken, SyntaxToken> found)
        {
            var condition = node as BinaryExpressionSyntax;
            var left = condition.Left;
            var right = condition.Right;
            var operatorToken = condition.OperatorToken;
            if (!judge(operatorToken.Kind()))
            {
                return false;
            }
            if (!(left is IdentifierNameSyntax leftIdName))
            {
                return false;
            }
            var leftToken = leftIdName.Identifier;
            var rightLiteralExpression = right as LiteralExpressionSyntax;
            var rightToken = rightLiteralExpression.Token;
            if (!rightToken.IsKind(SyntaxKind.NumericLiteralToken))
            {
                return false;
            }
            found(leftToken, operatorToken, rightToken);
            return true;
        }

        private static bool VariableDeclarationIsConstantInitializer(
            VariableDeclarationSyntax node,
            Action<SyntaxToken, SyntaxToken> found)
        {
            var allVariables = node.Variables;
            if (allVariables.Count != 1)
            {
                return false;
            }
            var variable = allVariables[0];
            var token = variable.Identifier;
            var initializer = variable.Initializer;
            if (initializer == null
                || !(initializer.Value is LiteralExpressionSyntax value))
            {
                return false;
            }
            var valueToken = value.Token;
            if (!valueToken.IsKind(SyntaxKind.NumericLiteralToken))
            {
                return false;
            }
            found(token, valueToken);
            return true;
        }
    }
}
