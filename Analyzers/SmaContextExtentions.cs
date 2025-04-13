namespace StyleChecker.Analyzers;

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Provides utility methods for <see cref="SemanticModelAnalysisContext"/>s.
/// </summary>
public static class SmaContextExtentions
{
    /// <summary>
    /// Gets the function that takes a <see cref="SyntaxNode"/> and returns the
    /// <see cref="IOperation"/> corresponding to the node, with the specified
    /// context.
    /// </summary>
    /// <param name="context">
    /// The semantic model analysis context.
    /// </param>
    /// <returns>
    /// The function that takes a <see cref="SyntaxNode"/> and returns the <see
    /// cref="IOperation"/> corresponding to the node.
    /// </returns>
    public static Func<SyntaxNode, IOperation?>
            GetOperationSupplier(this SemanticModelAnalysisContext context)
        => context.GetSymbolizer().GetOperation;

    /// <summary>
    /// Gets the function that takes a <see cref="SyntaxNode"/> and returns the
    /// <see cref="TypeInfo"/> corresponding to the node, with the specified
    /// context.
    /// </summary>
    /// <param name="context">
    /// The semantic model analysis context.
    /// </param>
    /// <returns>
    /// The function that takes a <see cref="SyntaxNode"/> and returns the <see
    /// cref="TypeInfo"/> corresponding to the node.
    /// </returns>
    public static Func<SyntaxNode, TypeInfo>
        GetTypeInfoSupplier(this SemanticModelAnalysisContext context)
    {
        var cancellationToken = context.CancellationToken;
        var model = context.SemanticModel;
        return e => model.GetTypeInfo(e, cancellationToken);
    }

    /// <summary>
    /// Gets the root of the syntax tree.
    /// </summary>
    /// <param name="context">
    /// The semantic model analysis context.
    /// </param>
    /// <returns>
    /// The root of the syntax tree.
    /// </returns>
    public static CompilationUnitSyntax
        GetCompilationUnitRoot(this SemanticModelAnalysisContext context)
    {
        var cancellationToken = context.CancellationToken;
        var model = context.SemanticModel;
        return model.SyntaxTree.GetCompilationUnitRoot(cancellationToken);
    }

    /// <summary>
    /// Gets the new <see cref="ISymbolizer"/> instance associated with the
    /// specified context.
    /// </summary>
    /// <param name="context">
    /// The semantic model analysis context.
    /// </param>
    /// <returns>
    /// The new <see cref="ISymbolizer"/> instance.
    /// </returns>
    public static ISymbolizer
            GetSymbolizer(this SemanticModelAnalysisContext context)
        => new Symbolizer(context.SemanticModel, context.CancellationToken);
}
