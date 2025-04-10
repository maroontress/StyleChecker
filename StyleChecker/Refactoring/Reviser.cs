namespace StyleChecker.Refactoring;

using Microsoft.CodeAnalysis;

/// <summary>
/// Revises a document.
/// </summary>
/// <param name="root">
/// The root of the syntax nodes.
/// </param>
/// <param name="node">
/// The syntax node to be replaced with a new node.
/// </param>
/// <param name="newNode">
/// The syntax node to replace a node with.
/// </param>
public sealed class Reviser(
    SyntaxNode root, SyntaxNode node, SyntaxNode newNode)
{
    /// <summary>
    /// Gets the root of the syntax nodes.
    /// </summary>
    public SyntaxNode Root { get; } = root;

    /// <summary>
    /// Gets the syntax node to be replaced with a new node.
    /// </summary>
    public SyntaxNode Node { get; } = node;

    /// <summary>
    /// Gets the syntax node to replace a node with.
    /// </summary>
    public SyntaxNode NewNode { get; } = newNode;
}
