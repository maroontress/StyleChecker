namespace CodeFixes.Refactoring.TypeClassParameter;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

/// <summary>
/// Provides utility methods for working with documents.
/// </summary>
public static class Documents
{
    /// <summary>
    /// Retrieves the semantic model, parameter symbol, and method symbol for
    /// the specified document and syntax node.
    /// </summary>
    /// <param name="document">
    /// The document to retrieve symbols from.
    /// </param>
    /// <param name="node">
    /// The syntax node to retrieve symbols for.
    /// </param>
    /// <param name="token">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// A tuple containing the semantic model, parameter symbol, and method
    /// symbol if found; otherwise, null.
    /// </returns>
    public static async Task<(
            SemanticModel Model,
            ISymbol Parameter,
            IMethodSymbol Method)?>
        GetSymbols(Document document, SyntaxNode node, CancellationToken token)
    {
        return (await document.GetSemanticModelAsync(token)
                .ConfigureAwait(false) is not {} model
                || model.GetDeclaredSymbol(node, token) is not {} parameter
                || parameter.ContainingSymbol is not IMethodSymbol method)
            ? null
            : (model, parameter, method);
    }
}
