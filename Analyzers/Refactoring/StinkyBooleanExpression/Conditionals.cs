namespace StyleChecker.Analyzers.Refactoring.StinkyBooleanExpression;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Provides utility methods for conditional expressions.
/// </summary>
public static class Conditionals
{
    /// <summary>
    /// Gets whether any of the true or false branches of the conditional
    /// expression has the specified syntax type.
    /// </summary>
    /// <param name="node">
    /// The conditional expression syntax node.
    /// </param>
    /// <param name="kind">
    /// The syntax kind to check for.
    /// </param>
    /// <returns>
    /// <c>true</c> if at least one branch has the specified syntax kind;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool AnyHasKindOf(
        this ConditionalExpressionSyntax node, SyntaxKind kind)
    {
        return node.WhenTrue.IsKind(kind)
            || node.WhenFalse.IsKind(kind);
    }
}
