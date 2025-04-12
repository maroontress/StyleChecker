namespace CodeFixes.Spacing;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// Provides utility methods for fixing tokens.
/// </summary>
public static class TokenFix
{
    /// <summary>
    /// Creates a new task that represents the asynchronous operation of fixing
    /// tokens in a document.
    /// </summary>
    /// <param name="newDocument">
    /// A function that returns the new document after fixing tokens.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation of fixing tokens.
    /// </returns>
    public static Func<CancellationToken, Task<Document>> NewTask(
        Func<Document> newDocument)
    {
        return c => Task.Run(newDocument, c);
    }

    /// <summary>
    /// Gets the new document created by adding a white space immediately after
    /// the specified token in the specified document.
    /// </summary>
    /// <param name="document">
    /// The document containing the <paramref name="token"/>.
    /// </param>
    /// <param name="root">
    /// The root node of the syntax tree of the <paramref name="document"/>.
    /// </param>
    /// <param name="token">
    /// The token to add a white space after.
    /// </param>
    /// <returns>
    /// The new document.
    /// </returns>
    public static Document AddSpaceAfterToken(
        Document document,
        SyntaxNode root,
        SyntaxToken token)
    {
        var list = token.TrailingTrivia;
        var space = SyntaxFactory.ParseTrailingTrivia(" ");
        var newList = list.InsertRange(0, space);
        var newToken = token.WithTrailingTrivia(newList);
        return Replace(document, root, token, newToken);
    }

    /// <summary>
    /// Gets the new document created by adding a white space immediately
    /// before the specified token in the specified document.
    /// </summary>
    /// <param name="document">
    /// The document containing the <paramref name="token"/>.
    /// </param>
    /// <param name="root">
    /// The root node of the syntax tree of the <paramref name="document"/>.
    /// </param>
    /// <param name="token">
    /// The token to add a white space before.
    /// </param>
    /// <returns>
    /// The new document.
    /// </returns>
    public static Document AddSpaceBeforeToken(
        Document document,
        SyntaxNode root,
        SyntaxToken token)
    {
        var list = token.LeadingTrivia;
        var space = SyntaxFactory.ParseLeadingTrivia(" ");
        var newList = list.AddRange(space);
        var newToken = token.WithLeadingTrivia(newList);
        return Replace(document, root, token, newToken);
    }

    private static Document Replace(
        Document document,
        SyntaxNode root,
        SyntaxToken token,
        SyntaxToken newToken)
    {
        var newRoot = root.ReplaceToken(token, newToken);
        return document.WithSyntaxRoot(newRoot);
    }
}
