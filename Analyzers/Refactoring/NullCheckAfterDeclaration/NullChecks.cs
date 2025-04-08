namespace Analyzers.Refactoring.NullCheckAfterDeclaration;

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Provides methods for checking null or non-null conditions in if statements.
/// </summary>
public static class NullChecks
{
    /// <summary>
    /// Gets the identifier name if the specified <c>if</c> statement is a null
    /// or non-null check.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A null check must be one of following patterns: <c>if (foo is null)
    /// ...</c>, <c>if (foo is not {}) ...</c>, <c>if (foo == null) ...</c>.
    /// </para>
    /// <para>
    /// A non-null check must be one of following patterns: <c>if (foo is not
    /// null)...</c>, <c>if (foo is {}) ...</c>, <c>if (foo != null) ...</c>.
    /// </para>
    /// </remarks>
    /// <param name="node">
    /// The if statement.
    /// </param>
    /// <returns>
    /// The identifier name when <paramref name="node"/> is a null or non-null
    /// check, <c>null</c> otherwise.
    /// </returns>
    public static IdentifierNameSyntax? IsNullOrNonNullCheck(
        IfStatementSyntax node)
    {
        /*
            [1] is null, is not null, is {}, is not {}:

            + IfStatement
              + Condition: IsPatternExpression
                + Expression: IdentifierName (*)
                + Pattern: (#1), (#2), or (#3)

            Pattern must be ont of the following:

            + Pattern: ConstantPattern (#1)
              + Expression: NullLiteralExpression

            + Pattern: RecursivePattern (#2)
              + PropertyPatternClause: PropertyPatternClause

            + Pattern: NotPattern (#3)
              + Pattern (#1) or (#2)
        */
        /*
            [2] == null, != null

            + IfStatement
              + Condition: EqualsExpression or NotEqualsExpression
                + Left: IdentifierName (*)
                + Right: NullLiteralExpression
        */

        static bool IsRightPattern(PatternSyntax p)
        {
            var pattern = (p is UnaryPatternSyntax unary
                    && unary.IsKind(SyntaxKind.NotPattern))
                ? unary.Pattern
                : p;
            return IsNullPattern(pattern)
                || IsEmptyClausePattern(pattern);
        }

        static bool IsRightBinaryExpr(BinaryExpressionSyntax binaryExpr)
        {
            return binaryExpr.IsKindOneOf(
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);
        }

        return node.Condition switch
        {
            BinaryExpressionSyntax binaryExpr
                => IsRightBinaryExpr(binaryExpr)
                    ? BinaryExprLeftIsIdRightIsNull(binaryExpr)
                    : null,
            IsPatternExpressionSyntax isPatternExpr
                => IsPatternExprLeftIsId(isPatternExpr, IsRightPattern),
            _ => null,
        };
    }

    /// <summary>
    /// Classifies the null check in the specified <c>if</c> statement.
    /// </summary>
    /// <param name="node">
    /// The <c>if</c> statement to classify.
    /// </param>
    /// <returns>
    /// <c>true</c> if the <c>if</c> statement is a null check; <c>false</c> if
    /// it is a non-null check; <c>null</c> otherwise.
    /// </returns>
    public static bool? ClassifyNullCheck(IfStatementSyntax node)
    {
        static bool RepesentsNullCheck(PatternSyntax p)
        {
            return IsNullPattern(p)
                || (p is UnaryPatternSyntax unary
                    && unary.IsKind(SyntaxKind.NotPattern)
                    && IsEmptyClausePattern(unary.Pattern));
        }

        static bool RepresentsNonNullCheck(PatternSyntax p)
        {
            return IsEmptyClausePattern(p)
                || (p is UnaryPatternSyntax unary
                    && unary.IsKind(SyntaxKind.NotPattern)
                    && IsNullPattern(unary.Pattern));
        }

        static bool? ClassifyPattern(PatternSyntax p)
        {
            return RepesentsNullCheck(p) ? true
                : RepresentsNonNullCheck(p) ? false
                : null;
        }

        static bool? ClassifyBinaryExpr(BinaryExpressionSyntax expr)
        {
            return expr.IsKind(SyntaxKind.EqualsExpression) ? true
                : expr.IsKind(SyntaxKind.NotEqualsExpression) ? false
                : null;
        }

        return node.Condition switch
        {
            BinaryExpressionSyntax binaryExpr
                => (BinaryExprLeftIsIdRightIsNull(binaryExpr) is {})
                    ? ClassifyBinaryExpr(binaryExpr)
                    : null,
            IsPatternExpressionSyntax isPatternExpr
                => (isPatternExpr.Expression is IdentifierNameSyntax)
                    ? ClassifyPattern(isPatternExpr.Pattern)
                    : null,
            _ => null,
        };
    }

    private static IdentifierNameSyntax? IsPatternExprLeftIsId(
            IsPatternExpressionSyntax isPattern,
            Func<PatternSyntax, bool> predicate)
        => ToNamePattern(isPattern) is {} p && predicate(p.Pattern)
            ? p.Name : null;

    private static IdentifierNameSyntax? BinaryExprLeftIsIdRightIsNull(
            BinaryExpressionSyntax binary)
        => (binary.Right is LiteralExpressionSyntax literal
                && literal.IsKind(SyntaxKind.NullLiteralExpression)
                && binary.Left is IdentifierNameSyntax identifierName)
            ? identifierName : null;

    private static bool IsNullPattern(PatternSyntax pattern)
        => pattern is ConstantPatternSyntax @const
            && @const.Expression.IsKind(SyntaxKind.NullLiteralExpression);

    private static bool IsEmptyClausePattern(PatternSyntax pattern)
        => pattern is RecursivePatternSyntax recursivePattern
            && recursivePattern.Designation is null
            && recursivePattern.PropertyPatternClause is {} propertyClause
            && propertyClause.Subpatterns.Count is 0;

    private static NamePattern? ToNamePattern(
            IsPatternExpressionSyntax isPattern)
        => isPattern.Expression is IdentifierNameSyntax name
            ? new(name, isPattern.Pattern) : null;

    private sealed class NamePattern(
        IdentifierNameSyntax name, PatternSyntax pattern)
    {
        public IdentifierNameSyntax Name { get; } = name;

        public PatternSyntax Pattern { get; } = pattern;
    }
}
