namespace StyleChecker.Refactoring.NullCheckAfterDeclaration;

using System.Linq;
using Microsoft.CodeAnalysis;

/// <summary>
/// Provides methods for syntax nodes.
/// </summary>
public static class SyntaxNodes
{
    /// <summary>
    /// Gets the next sibling node of the specified node.
    /// </summary>
    /// <param name="node">
    /// The node to get the next sibling of.
    /// </param>
    /// <returns>]
    /// The next sibling node, or <c>null</c> if there is no next sibling.
    /// </returns>
    public static SyntaxNode? NextNode(this SyntaxNode node)
    {
        return (node.Parent is not {} parent)
            ? null
            : parent.ChildNodes()
                .SkipWhile(n => n != node)
                .Skip(1)
                .FirstOrDefault();
    }
}
