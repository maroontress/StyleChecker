namespace StyleChecker.Refactoring;

using System;
using System.Threading;
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
    {
        return new SymbolizerImpl(context);
    }

    private class SymbolizerImpl(SemanticModelAnalysisContext context)
        : ISymbolizer
    {
        private SemanticModel Model { get; } = context.SemanticModel;

        private CancellationToken CancellationToken { get; }
            = context.CancellationToken;

        public IOperation? GetOperation(SyntaxNode node)
            => Model.GetOperation(node, CancellationToken);

        public IMethodSymbol? ToSymbol(AccessorDeclarationSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public IMethodSymbol? ToSymbol(BaseMethodDeclarationSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public ILocalSymbol? ToSymbol(CatchDeclarationSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public ILocalSymbol? ToSymbol(ForEachStatementSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public ITypeParameterSymbol? ToSymbol(TypeParameterSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public IParameterSymbol? ToSymbol(ParameterSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public IAliasSymbol? ToSymbol(ExternAliasDirectiveSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public IAliasSymbol? ToSymbol(UsingDirectiveSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public INamespaceSymbol? ToSymbol(NamespaceDeclarationSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public INamedTypeSymbol? ToSymbol(BaseTypeDeclarationSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public INamedTypeSymbol? ToSymbol(DelegateDeclarationSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public INamedTypeSymbol? ToSymbol(
            AnonymousObjectCreationExpressionSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public INamedTypeSymbol? ToSymbol(TupleExpressionSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public IFieldSymbol? ToSymbol(EnumMemberDeclarationSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public IPropertySymbol? ToSymbol(PropertyDeclarationSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public IPropertySymbol? ToSymbol(IndexerDeclarationSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public IPropertySymbol? ToSymbol(
                AnonymousObjectMemberDeclaratorSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public ILabelSymbol? ToSymbol(SwitchLabelSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public ILabelSymbol? ToSymbol(LabeledStatementSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public IEventSymbol? ToSymbol(EventDeclarationSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public IRangeVariableSymbol? ToSymbol(JoinIntoClauseSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public IRangeVariableSymbol? ToSymbol(QueryClauseSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public IRangeVariableSymbol? ToSymbol(QueryContinuationSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public ISymbol? ToSymbol(VariableDeclaratorSyntax node)
            => Model.GetDeclaredSymbol(node, CancellationToken);

        public TypeInfo ToTypeInfo(SyntaxNode node)
            => Model.GetTypeInfo(node, CancellationToken);

        public DataFlowAnalysis ToDataFlowAnalysis(SyntaxNode node)
            => Model.AnalyzeDataFlow(node);
    }
}
