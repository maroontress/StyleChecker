namespace StyleChecker.Refactoring
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Creates a Reviser instance.
    /// </summary>
    public sealed class ReviserKit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReviserKit"/> class.
        /// </summary>
        /// <param name="key">
        /// The key representing the fix title.
        /// </param>
        /// <param name="toRefactor">
        /// The funciton that accepts the root node and the text span, and
        /// returns the Reviser instance.
        /// </param>
        public ReviserKit(
            string key,
            Func<SyntaxNode, TextSpan, Reviser?> toRefactor)
        {
            Key = key;
            ToReviser = toRefactor;
        }

        /// <summary>
        /// Gets the key representing the fix title.
        /// </summary>
        public string Key { get; }

        private Func<SyntaxNode, TextSpan, Reviser?> ToReviser { get; }

        /// <summary>
        /// Gets the new reviser instance.
        /// </summary>
        /// <param name="root">
        /// The root of syntax nodes.
        /// </param>
        /// <param name="span">
        /// The text span representing the syntax node to revise.
        /// </param>
        /// <returns>
        /// The reviser instance if the span is valid. Otherwise, <c>null</c>.
        /// </returns>
        public Reviser? FindReviser(SyntaxNode root, TextSpan span)
        {
            return ToReviser(root, span);
        }
    }
}
