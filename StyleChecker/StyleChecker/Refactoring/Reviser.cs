namespace StyleChecker.Refactoring
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Revises a document.
    /// </summary>
    public sealed class Reviser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Reviser"/> class.
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
        public Reviser(SyntaxNode root, SyntaxNode node, SyntaxNode newNode)
        {
            Root = root;
            Node = node;
            NewNode = newNode;
        }

        /// <summary>
        /// Gets the root of the syntax nodes.
        /// </summary>
        public SyntaxNode Root { get; }

        /// <summary>
        /// Gets the syntax node to be replaced with a new node.
        /// </summary>
        public SyntaxNode Node { get; }

        /// <summary>
        /// Gets the syntax node to replace a node with.
        /// </summary>
        public SyntaxNode NewNode { get; }
    }
}
