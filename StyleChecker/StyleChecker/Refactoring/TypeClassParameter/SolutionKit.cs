namespace StyleChecker.Refactoring.TypeClassParameter;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Rename;

/// <summary>
/// A kit to create a new solution for refactoring type class parameters.
/// </summary>
/// <param name="token">
/// The cancellation token.
/// </param>
/// <param name="solution">
/// The solution.
/// </param>
/// <param name="document">
/// The document.
/// </param>
/// <param name="typeName">
/// The type name such as "T", "T0", "T1", and so on.
/// </param>
public class SolutionKit(
    CancellationToken token,
    Solution solution,
    Document document,
    string typeName)
{
    /// <summary>
    /// Gets the cancellation token.
    /// </summary>
    private CancellationToken CancellationToken { get; } = token;

    /// <summary>
    /// Gets the solution.
    /// </summary>
    private Solution Solution { get; } = solution;

    /// <summary>
    /// Gets the document.
    /// </summary>
    private Document Document { get; } = document;

    /// <summary>
    /// Gets the type name.
    /// </summary>
    private string TypeName { get; } = typeName;

    /// <summary>
    /// Gets the new solution where the specified method symbol is renamed and
    /// then the type parameter is added to the method.
    /// </summary>
    /// <param name="symbol">
    /// The method symbol to be renamed to "name_0", "name_1", and so on.
    /// </param>
    /// <param name="name">
    /// The symbol's name.
    /// </param>
    /// <param name="allSymbolNameSet">
    /// The set of all symbol names that have already been used.
    /// </param>
    /// <param name="documentId">
    /// The document ID.
    /// </param>
    /// <param name="realNode">
    /// The syntax node being tracked.
    /// </param>
    /// <returns>
    /// The renamed solution.
    /// </returns>
    public async Task<Solution?> GetRenamedSolution(
        IMethodSymbol symbol,
        string name,
        ISet<string> allSymbolNameSet,
        DocumentId documentId,
        SyntaxNode realNode)
    {
        if (GetRenamingIndex(allSymbolNameSet, 0, name) is not {} k)
        {
            return null;
        }
        var options = default(SymbolRenameOptions);
        var newSolution = await Renamer.RenameSymbolAsync(
                Solution,
                symbol,
                options,
                $"{name}_{k}",
                CancellationToken)
            .ConfigureAwait(false);
        var projectId = Document.Project.Id;
        var project = newSolution.GetProject(projectId);
        if (project is null)
        {
            return null;
        }
        var newDocument = project.Documents
            .Where(d => d.Id == documentId)
            .First();
        if (await newDocument.GetSyntaxRootAsync(CancellationToken)
                .ConfigureAwait(false) is not {} root
            || root.GetCurrentNode(realNode) is not {} node
            || await Documents.GetSymbols(newDocument, CancellationToken, node)
                .ConfigureAwait(false) is not {} symbols)
        {
            return null;
        }
        var kit = new SolutionKit(
            CancellationToken, newSolution, newDocument, TypeName);
        return await kit.GetNewSolution(
            symbols.Parameter, symbols.Method, root);
    }

    /// <summary>
    /// Gets the new solution where the type parameter is added to the method.
    /// </summary>
    /// <param name="parameterSymbol">
    /// The parameter symbol.
    /// </param>
    /// <param name="methodSymbol">
    /// The method symbol.
    /// </param>
    /// <param name="root">
    /// The syntax root.
    /// </param>
    /// <returns>
    /// The new solution.
    /// </returns>
    public async Task<Solution?> GetNewSolution(
        ISymbol parameterSymbol,
        IMethodSymbol methodSymbol,
        SyntaxNode root)
    {
        var parameterArray = methodSymbol.Parameters
            .ToArray();
        var index = Array.FindIndex(
            parameterArray, p => Symbols.AreEqual(p, parameterSymbol));
        if (index == -1)
        {
            return null;
        }

        var allReferences = await SymbolFinder.FindReferencesAsync(
                methodSymbol, Solution, CancellationToken)
            .ConfigureAwait(false);
        var documentGroups = allReferences.SelectMany(r => r.Locations)
            .GroupBy(w => w.Document);
        var newRoot = DocumentUpdater.UpdateMainDocument(
            TypeName,
            Document,
            root,
            methodSymbol,
            index,
            documentGroups);
        if (newRoot is null)
        {
            return null;
        }
        var workspace = Solution.Workspace;
        var formattedNode = Formatter.Format(
           newRoot,
           Formatter.Annotation,
           workspace,
           workspace.Options);
        var newSolution = Solution.WithDocumentSyntaxRoot(
            Document.Id, formattedNode);
        return await DocumentUpdater.UpdateReferencingDocumentsAsync(
                Document,
                index,
                documentGroups,
                newSolution,
                CancellationToken)
            .ConfigureAwait(false);
    }

    private static int? GetRenamingIndex(
        ISet<string> set, int start, string name)
    {
        for (var k = start; k >= 0; ++k)
        {
            var n = $"{name}_{k}";
            if (!set.Contains(n))
            {
                set.Add(n);
                return k;
            }
        }
        return null;
    }
}
