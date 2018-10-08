namespace StyleChecker.Refactoring.IneffectiveReadByte
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static partial class ForStatements
    {
        public static LoopIndexRange GetLoopIndexRange(
            SemanticModel model, SyntaxNode node)
        {
            var forNode = node as ForStatementSyntax;
            if (forNode == null)
            {
                return null;
            }
            var declaration = forNode.Declaration;
            var allInitializers = forNode.Initializers;
            var condition = forNode.Condition;
            var allIncrementors = forNode.Incrementors;
            var statement = forNode.Statement;

            if (allInitializers.Count != 0)
            {
                return null;
            }
            if (allIncrementors.Count != 1)
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
            var symbol = model.LookupSymbols(span.Start, null, token.Text)
                .FirstOrDefault() as ILocalSymbol;
            if (symbol == null)
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
            if (isWrittenInsideLoop)
            {
                return null;
            }
            return new LoopIndexRange(symbol, context.Start, context.End);
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
            var idName = operand as IdentifierNameSyntax;
            if (idName == null)
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
            var leftIdName = left as IdentifierNameSyntax;
            if (leftIdName == null)
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
            var argumentList = variable.ArgumentList;
            var initializer = variable.Initializer;
            var initializerValue = initializer.Value as LiteralExpressionSyntax;
            if (initializerValue == null)
            {
                return false;
            }
            var valueToken = initializerValue.Token;
            if (!valueToken.IsKind(SyntaxKind.NumericLiteralToken))
            {
                return false;
            }
            found(token, valueToken);
            return true;
        }
    }
}
