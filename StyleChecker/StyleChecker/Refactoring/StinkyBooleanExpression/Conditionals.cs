namespace StyleChecker.Refactoring.StinkyBooleanExpression;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Provides utility methods for conditional expressions.
/// </summary>
public static class Conditionals
{
    /// <summary>
    /// Gets whether both the true and false branches of a conditional
    /// expression have the specified syntax kind.
    /// </summary>
    /// <param name="node">
    /// The conditional expression syntax node.
    /// </param>
    /// <param name="kind">
    /// The syntax kind to check for.
    /// </param>
    /// <returns>
    /// <c>true</c> if both branches have the specified syntax kind; otherwise,
    /// <c>false</c>.
    /// </returns>
    public static bool BothIsKind(
        this ConditionalExpressionSyntax node, SyntaxKind kind)
    {
        return node.WhenTrue.IsKind(kind)
            || node.WhenFalse.IsKind(kind);
    }
}
