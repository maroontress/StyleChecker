namespace Analyzers;

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Supplies the <see cref="ISymbol"/> objects associated with the <see
/// cref="SyntaxNode"/>.
/// </summary>
public sealed class Symbolizer(
    SemanticModel model, CancellationToken cancellationToken)
    : ISymbolizer
{
    private SemanticModel Model { get; } = model;

    private CancellationToken CancellationToken { get; }
        = cancellationToken;

    /// <inheritdoc/>
    public IOperation? GetOperation(SyntaxNode node)
        => Model.GetOperation(node, CancellationToken);

    /// <inheritdoc/>
    public IMethodSymbol? ToSymbol(AccessorDeclarationSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public IMethodSymbol? ToSymbol(BaseMethodDeclarationSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public ILocalSymbol? ToSymbol(CatchDeclarationSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public ILocalSymbol? ToSymbol(ForEachStatementSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public ITypeParameterSymbol? ToSymbol(TypeParameterSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public IParameterSymbol? ToSymbol(ParameterSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public IAliasSymbol? ToSymbol(ExternAliasDirectiveSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public IAliasSymbol? ToSymbol(UsingDirectiveSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public INamespaceSymbol? ToSymbol(NamespaceDeclarationSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public INamedTypeSymbol? ToSymbol(BaseTypeDeclarationSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public INamedTypeSymbol? ToSymbol(DelegateDeclarationSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public INamedTypeSymbol? ToSymbol(
        AnonymousObjectCreationExpressionSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public INamedTypeSymbol? ToSymbol(TupleExpressionSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public IFieldSymbol? ToSymbol(EnumMemberDeclarationSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public IPropertySymbol? ToSymbol(PropertyDeclarationSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public IPropertySymbol? ToSymbol(IndexerDeclarationSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public IPropertySymbol? ToSymbol(
            AnonymousObjectMemberDeclaratorSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public ILabelSymbol? ToSymbol(SwitchLabelSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public ILabelSymbol? ToSymbol(LabeledStatementSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public IEventSymbol? ToSymbol(EventDeclarationSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public IRangeVariableSymbol? ToSymbol(JoinIntoClauseSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public IRangeVariableSymbol? ToSymbol(QueryClauseSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public IRangeVariableSymbol? ToSymbol(QueryContinuationSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public ISymbol? ToSymbol(VariableDeclaratorSyntax node)
        => Model.GetDeclaredSymbol(node, CancellationToken);

    /// <inheritdoc/>
    public TypeInfo ToTypeInfo(SyntaxNode node)
        => Model.GetTypeInfo(node, CancellationToken);

    /// <inheritdoc/>
    public DataFlowAnalysis ToDataFlowAnalysis(SyntaxNode node)
        => Model.AnalyzeDataFlow(node);

    /// <inheritdoc/>
    public ControlFlowAnalysis ToControlFlowAnalysis(SyntaxNode node)
        => Model.AnalyzeControlFlow(node);

    /// <inheritdoc/>
    public NullableContext GetNullableContext(int position)
        => Model.GetNullableContext(position);
}
