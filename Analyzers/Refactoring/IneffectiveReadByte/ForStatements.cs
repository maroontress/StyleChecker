namespace Analyzers.Refactoring.IneffectiveReadByte;

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
    /// name="node"/> is a <c>for</c> statement and the range of the loop index
    /// is constant, <c>null</c> otherwise.
    /// </returns>
    public static LoopIndexRange? GetLoopIndexRange(
        SemanticModel model, SyntaxNode node)
    {
        static bool IsLessThanOrLessThanEquals(SyntaxKind kind)
            => kind == SyntaxKind.LessThanToken
                || kind == SyntaxKind.LessThanEqualsToken;

        if (node is not ForStatementSyntax forNode
            || forNode.Declaration is not {} declaration
            || forNode.Condition is not {} condition
            || forNode.Initializers is not { Count: 0 } allInitializers
            || forNode.Incrementors is not { Count: 1 } allIncrementors)
        {
            return null;
        }
        var incrementor = allIncrementors[0];

        var context = new ForLoopIndexRangeContext();
        if (!VariableDeclarationIsConstantInitializer(
                model, declaration, context.First)
            || !ExpressionIsBinaryLeftIdRightNumber(
                model,
                condition,
                IsLessThanOrLessThanEquals,
                context.Second)
            || !ExpressionIsPreOrPostIncrement(incrementor, context.Third)
            || !context.IsValid())
        {
            return null;
        }
        var token = context.Id;
        var span = token.Span;
        return (model.LookupSymbols(span.Start, null, token.Text)
                .FirstOrDefault() is not ILocalSymbol symbol
                || symbol.Type.SpecialType is not SpecialType.System_Int32
                || model.AnalyzeDataFlow(forNode.Statement) is not {} dataFlow
                || dataFlow.WrittenInside
                    .Any(s => Symbols.AreEqual(s, symbol)))
            ? null
            : new LoopIndexRange(symbol, context.Start, context.End);
    }

    private static bool ExpressionIsPreOrPostIncrement(
        ExpressionSyntax node, Action<SyntaxToken> found)
    {
        var maybeTuple = node switch
        {
            PrefixUnaryExpressionSyntax pre
                => (pre.OperatorToken, pre.Operand),
            PostfixUnaryExpressionSyntax post
                => (post.OperatorToken, post.Operand),
            _ => ((SyntaxToken, ExpressionSyntax)?)null,
        };
        if (maybeTuple is not {} tuple)
        {
            return false;
        }
        var (operatorToken, operand) = tuple;
        if (!operatorToken.IsKind(SyntaxKind.PlusPlusToken)
            || operand is not IdentifierNameSyntax idName)
        {
            return false;
        }
        found(idName.Identifier);
        return true;
    }

    private static bool ExpressionIsBinaryLeftIdRightNumber(
        SemanticModel model,
        ExpressionSyntax node,
        Func<SyntaxKind, bool> judge,
        Action<SyntaxToken, SyntaxToken, int> found)
    {
        if (node is not BinaryExpressionSyntax condition)
        {
            return false;
        }
        var left = condition.Left;
        var right = condition.Right;
        var operatorToken = condition.OperatorToken;
        if (!judge(operatorToken.Kind())
            || left is not IdentifierNameSyntax leftIdName
            || right is not LiteralExpressionSyntax rightLiteralExpression)
        {
            return false;
        }
        var leftToken = leftIdName.Identifier;
        var rightToken = rightLiteralExpression.Token;
        if (!rightToken.IsKind(SyntaxKind.NumericLiteralToken)
            || model.GetConstantValue(right).Value is not int intValue)
        {
            return false;
        }
        found(leftToken, operatorToken, intValue);
        return true;
    }

    private static bool VariableDeclarationIsConstantInitializer(
        SemanticModel model,
        VariableDeclarationSyntax node,
        Action<SyntaxToken, int> found)
    {
        var allVariables = node.Variables;
        if (allVariables.Count is not 1)
        {
            return false;
        }
        var variable = allVariables[0];
        var token = variable.Identifier;
        if (variable.Initializer is not {} initializer
            || initializer.Value is not LiteralExpressionSyntax value)
        {
            return false;
        }
        var valueToken = value.Token;
        if (!valueToken.IsKind(SyntaxKind.NumericLiteralToken))
        {
            return false;
        }
        if (model.GetConstantValue(value).Value is not int intValue)
        {
            return false;
        }
        found(token, intValue);
        return true;
    }
}
