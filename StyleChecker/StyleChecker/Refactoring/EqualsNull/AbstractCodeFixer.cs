namespace StyleChecker.Refactoring.EqualsNull
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using R = Resources;

    /// <summary>
    /// Provides abstraction of the CodeFix provider that replaces a
    /// BinaryExpressionSyntax node with a SyntaxNode.
    /// </summary>
    public abstract class AbstractCodeFixer : AbstractRevisingCodeFixer
    {
        /// <inheritdoc/>
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(Analyzer.DiagnosticId);

        /// <inheritdoc/>
        protected sealed override Func<string, LocalizableString> Localize
            => Localizers.Of<R>(R.ResourceManager);

        /// <summary>
        /// Gets a new 'is' token.
        /// </summary>
        /// <param name="node">
        /// The node from which the token copys the leading and trailing trivias.
        /// </param>
        /// <returns>
        /// The 'is' token.
        /// </returns>
        protected static SyntaxToken NewIsToken(BinaryExpressionSyntax node)
        {
            return SyntaxFactory.Token(SyntaxKind.IsKeyword)
                 .WithTriviaFrom(node.OperatorToken);
        }

        /// <summary>
        /// Gets the function that accepts the SyntaxNode and TextSpan
        /// representing BinaryExpressionSyntax, and returns a Reviser instance
        /// or null, associated with the specified SyntaxKind and the specified
        /// function.
        /// </summary>
        /// <param name="kind">
        /// The SyntaxKind that the SyntaxNode to be replaced should be of.
        /// </param>
        /// <param name="toNewNode">
        /// The function that accepts the BinaryExpressionSyntax and returns a
        /// SyntaxNode.
        /// </param>
        /// <returns>
        /// The function that accepts the SyntaxNode and TextSpan, and returns
        /// a Reviser instance or null.
        /// </returns>
        protected static Func<SyntaxNode, TextSpan, Reviser?> Replace(
            SyntaxKind kind,
            Func<BinaryExpressionSyntax, SyntaxNode> toNewNode)
        {
            return (SyntaxNode root, TextSpan span) =>
            {
                var node = root.FindNodeOfType<BinaryExpressionSyntax>(span);
                if (node is null)
                {
                    return null;
                }
                if (!node.OperatorToken
                    .IsKind(kind))
                {
                    return null;
                }
                var newNode = toNewNode(node);
                return new Reviser(root, node, newNode);
            };
        }
    }
}
