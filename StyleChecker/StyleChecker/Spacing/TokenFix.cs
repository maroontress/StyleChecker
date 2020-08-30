namespace StyleChecker.Spacing
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Provides utility methods for fixing tokens.
    /// </summary>
    public static class TokenFix
    {
        /// <summary>
        /// Gets the task that returns a new document created by adding a white
        /// space immediately after the specified token in the specified
        /// document.
        /// </summary>
        /// <param name="document">
        /// The document containing the <paramref name="token"/>.
        /// </param>
        /// <param name="token">
        /// The token to add a white space after.
        /// </param>
        /// <returns>
        /// The new document.
        /// </returns>
        public static Task<Document> AddSpaceAfterToken(
            Document document,
            SyntaxToken token)
        {
            var list = token.TrailingTrivia;
            var space = SyntaxFactory.ParseTrailingTrivia(" ");
            var newList = list.InsertRange(0, space);
            var newToken = token.WithTrailingTrivia(newList);
            return Replace(document, token, newToken);
        }

        /// <summary>
        /// Gets the task that returns a new document created by adding a white
        /// space immediately before the specified token in the specified
        /// document.
        /// </summary>
        /// <param name="document">
        /// The document containing the <paramref name="token"/>.
        /// </param>
        /// <param name="token">
        /// The token to add a white space before.
        /// </param>
        /// <returns>
        /// The new document.
        /// </returns>
        public static Task<Document> AddSpaceBeforeToken(
            Document document,
            SyntaxToken token)
        {
            var list = token.LeadingTrivia;
            var space = SyntaxFactory.ParseLeadingTrivia(" ");
            var newList = list.AddRange(space);
            var newToken = token.WithLeadingTrivia(newList);
            return Replace(document, token, newToken);
        }

        private static async Task<Document> Replace(
            Document document,
            SyntaxToken token,
            SyntaxToken newToken)
        {
            var root = token.SyntaxTree
                .GetRoot();
            var newRoot = root.ReplaceToken(token, newToken);
            return await Task.Run(() => document.WithSyntaxRoot(newRoot))
                .ConfigureAwait(false);
        }
    }
}
