namespace StyleChecker.Analyzers.Refactoring.NullCheckAfterDeclaration;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Provides utility methods for working with C# expression syntaxes.
/// </summary>
public static class Expressions
{
    /// <summary>
    /// Peels off any parenthesized expressions to get the core expression.
    /// </summary>
    /// <param name="expr">
    /// The expression to peel.
    /// </param>
    /// <returns>
    /// The core expression without any parentheses.
    /// </returns>
    public static ExpressionSyntax Peel(ExpressionSyntax expr)
    {
        var s = expr;
        while (s is ParenthesizedExpressionSyntax p)
        {
            s = p.Expression;
        }
        return s;
    }

    /// <summary>
    /// Creates a parenthesized expression from the specified expression if it
    /// isn't already parenthesized.
    /// </summary>
    /// <param name="expr">
    /// The expression to potentially parenthesize.
    /// </param>
    /// <returns>
    /// The specified expression if it's already a <see
    /// cref="ParenthesizedExpressionSyntax"/>; otherwise, a new parenthesized
    /// expression containing the specified expression.
    /// </returns>
    public static ParenthesizedExpressionSyntax ParenthesizeIfNeeded(
        ExpressionSyntax expr)
    {
        return expr is ParenthesizedExpressionSyntax parenthesized
            ? parenthesized
            : SyntaxFactory.ParenthesizedExpression(expr);
    }
}
