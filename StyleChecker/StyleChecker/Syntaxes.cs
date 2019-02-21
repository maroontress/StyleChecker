namespace StyleChecker
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Provides extension methods for <c>SyntaxNode</c>, <c>SyntaxTrivia</c>,
    /// <c>SyntaxToken</c>, and so on.
    /// </summary>
    public static class Syntaxes
    {
        /// <summary>
        /// Returns whether the <c>SyntaxKind</c> of the specified
        /// <c>SyntaxTrivia</c> is included in the specified
        /// <c>SyntaxKind</c>s or not.
        /// </summary>
        /// <param name="trivia">
        /// The <c>SyntaxTrivia</c> object.
        /// </param>
        /// <param name="kinds">
        /// <c>SyntaxKind</c> objects.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <c>SyntaxKind</c> of <paramref name="trivia"/>
        /// is included in <paramref name="kinds"/>, <c>false</c> otherwise.
        /// </returns>
        public static bool IsKindOneOf(
            this SyntaxTrivia trivia, params SyntaxKind[] kinds)
        {
            return kinds.Select(k => trivia.IsKind(k))
                .FirstOrDefault(b => b);
        }

        /// <summary>
        /// Returns whether the <c>SyntaxKind</c> of the specified
        /// <c>SyntaxNode</c> is included in the specified
        /// <c>SyntaxKind</c>s or not.
        /// </summary>
        /// <param name="node">
        /// The <c>SyntaxNode</c> object.
        /// </param>
        /// <param name="kinds">
        /// <c>SyntaxKind</c> objects.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <c>SyntaxKind</c> of <paramref name="node"/>
        /// is included in <paramref name="kinds"/>, <c>false</c> otherwise.
        /// </returns>
        public static bool IsKindOneOf(
            this SyntaxNode node, params SyntaxKind[] kinds)
        {
            return kinds.Select(k => node.IsKind(k))
                .FirstOrDefault(b => b);
        }

        /// <summary>
        /// Gets the node of the specified type, corresponging to the
        /// specified span.
        /// </summary>
        /// <typeparam name="T">
        /// The type of node.
        /// </typeparam>
        /// <param name="root">
        /// The node to start finding.
        /// </param>
        /// <param name="span">
        /// The span to specify the node.
        /// </param>
        /// <returns>
        /// The node if found, <c>null</c> otherwise.
        /// </returns>
        public static T FindNodeOfType<T>(
            this SyntaxNode root, TextSpan span)
            where T : SyntaxNode
        {
            var outermostNode = root.FindNode(span);
            var node = root.FindNode(span, getInnermostNodeForTie: true);
            while (!(node is T) && node != outermostNode)
            {
                node = node.Parent;
            }
            return (node is T foundNode) ? foundNode : null;
        }
    }
}
