namespace StyleChecker.CodeFixes.Refactoring.TypeClassParameter;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maroontress.Roastery;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Rename;
using StyleChecker.Analyzers;

/// <summary>
/// A kit to create a new solution for refactoring type class parameters.
/// </summary>
/// <param name="solution">
/// The solution.
/// </param>
/// <param name="document">
/// The document.
/// </param>
/// <param name="typeName">
/// The type name such as "T", "T0", "T1", and so on.
/// </param>
/// <param name="token">
/// The cancellation token.
/// </param>
public class SolutionKit(
    Solution solution,
    Document document,
    string typeName,
    CancellationToken token)
{
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
    /// Gets the cancellation token.
    /// </summary>
    private CancellationToken CancellationToken { get; } = token;

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
    /// <param name="node">
    /// The parameter syntax node (of a method or a local function).
    /// </param>
    /// <returns>
    /// The renamed solution.
    /// </returns>
    public async Task<Solution?> GetRenamedSolution(
        IMethodSymbol symbol,
        string name,
        ISet<string> allSymbolNameSet,
        DocumentId documentId,
        ParameterSyntax node)
    {
        if (await Document.GetSemanticModelAsync(CancellationToken)
                .ConfigureAwait(false) is not {} model)
        {
            return null;
        }
        var symbolier = new Symbolizer(model, CancellationToken);
        if (GetRenamingIndex(allSymbolNameSet, 0, name) is not {} k
            || symbolier.ToSymbol(node) is not {} parameter)
        {
            return null;
        }
        var options = default(SymbolRenameOptions);
        var newName = $"{name}_{k}";

        /*
            Note:

            SyntaxNode.TrackNodes() and SyntaxNode.GetCurrentNode() do not
            always work before and after a call to Renamer.RenameSymbolAsync()
            (especially not in Visual Studio IDE). Instead, use
            SymbolFinder.FindSimilarSymbols() to track nodes by their
            corresponding symbols.
        */
        var newSolution = await RenameSymbolAsync(
            Solution, symbol, options, newName);
        var projectId = Document.Project.Id;
        if (newSolution.GetProject(projectId) is not {} newProject
            || newProject.Documents
                .FirstOrDefault(d => d.Id == documentId) is not {} newDocument
            || await GetSyntaxRootAsync(newDocument) is not {} newRoot
            || await GetCompilationAsync(newProject) is not {} newCompilation
            || FindFirstSimilarSymbol(parameter, newCompilation)
                is not {} newParameter
            || newParameter.DeclaringSyntaxReferences
                .Select(async r => await GetSyntaxAsync(r))
                .FirstOrDefault() is not {} newNode
            || newParameter.ContainingSymbol is not IMethodSymbol newMethod)
        {
            return newSolution;
        }
        var kit = new SolutionKit(
            newSolution, newDocument, TypeName, CancellationToken);
        return await kit.GetNewSolution(newParameter, newMethod, newRoot);
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
        if (methodSymbol.Parameters
            .WithIndex()
            .Where(p => Symbols.AreEqual(p.Value, parameterSymbol))
            .FirstValue() is not {} parameter)
        {
            return null;
        }
        var index = parameter.Index;

        var allReferences = await FindReferencesAsync(methodSymbol, Solution);
        var documentGroups = allReferences.SelectMany(r => r.Locations)
            .GroupBy(w => w.Document);
        if (DocumentUpdater.UpdateMainDocument(
            TypeName,
            Document,
            root,
            methodSymbol,
            index,
            documentGroups) is not {} newRoot)
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
        if (start < 0)
        {
            return null;
        }
        var all = (start is 0)
            ? Enumerable.Range(1, int.MaxValue)
                .Prepend(0)
            : Enumerable.Range(start, int.MaxValue - start + 1);
        if (all.Select(i => (Index: i, Id: $"{name}_{i}"))
            .Where(s => !set.Contains(s.Id))
            .FirstValue() is not {} p)
        {
            return null;
        }
        var (index, id) = p;
        set.Add(id);
        return index;
    }

    private async Task<Solution> RenameSymbolAsync(
        Solution solution,
        ISymbol symbol,
        SymbolRenameOptions options,
        string newName)
    {
        return await Renamer.RenameSymbolAsync(
                solution, symbol, options, newName, CancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<SyntaxNode?> GetSyntaxRootAsync(Document document)
    {
        return await document.GetSyntaxRootAsync(CancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<Compilation?> GetCompilationAsync(Project project)
    {
        return await project.GetCompilationAsync(CancellationToken)
            .ConfigureAwait(false);
    }

    private T FindFirstSimilarSymbol<T>(T symbol, Compilation compilation)
        where T : ISymbol
    {
        return SymbolFinder.FindSimilarSymbols(
                symbol, compilation, CancellationToken)
            .FirstOrDefault();
    }

    private async Task<IEnumerable<ReferencedSymbol>> FindReferencesAsync(
        ISymbol symbol, Solution solution)
    {
        return await SymbolFinder.FindReferencesAsync(
                symbol, solution, CancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<SyntaxNode> GetSyntaxAsync(SyntaxReference reference)
    {
        return await reference.GetSyntaxAsync(CancellationToken)
            .ConfigureAwait(false);
    }
}
