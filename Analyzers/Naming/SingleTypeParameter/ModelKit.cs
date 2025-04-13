namespace StyleChecker.Analyzers.Naming.SingleTypeParameter;

using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Represents a model kit that provides various utilities for working with
/// semantic models in Roslyn.
/// </summary>
/// <param name="context">
/// The context for semantic model analysis.
/// </param>
public sealed class ModelKit(SemanticModelAnalysisContext context)
{
    private SemanticModel Model { get; } = context.SemanticModel;

    private CancellationToken CancellationToken { get; }
        = context.CancellationToken;

    /// <summary>
    /// Gets the compilation unit syntax of this model.
    /// </summary>
    /// <returns>
    /// The <see cref="CompilationUnitSyntax"/> instance.
    /// </returns>
    public CompilationUnitSyntax GetCompilationUnitRoot()
        => Model.SyntaxTree.GetCompilationUnitRoot(CancellationToken);

    /// <summary>
    /// Gets the full name of the symbol associated with the specified syntax
    /// token.
    /// </summary>
    /// <param name="token">
    /// The syntax token to get the symbol's full name for.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{String}"/> instance containing the full name
    /// of the symbol when the symbol exists, being empty otherwise.
    /// </returns>
    public IEnumerable<string> GetFullName(SyntaxToken token)
        => token.Parent is {} parent
            && Model.GetSymbolInfo(parent, CancellationToken)
                .Symbol is {} symbol
        ? [symbol.ToString()]
        : [];

    /// <summary>
    /// Gets the declared symbol for the specified type declaration syntax
    /// node.
    /// </summary>
    /// <param name="node">
    /// The type declaration syntax node to get the declared symbol for.
    /// </param>
    /// <returns>
    /// The <see cref="ISymbol"/> instance representing the declared symbol for
    /// the specified node, or <c>null</c> if no symbol is declared.
    /// </returns>
    public ISymbol? GetDeclaredSymbol(TypeDeclarationSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);
}
