namespace StyleChecker.Refactoring;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Supplies the <see cref="ISymbol"/> objects associated with the <see
/// cref="SyntaxNode"/>.
/// </summary>
public interface ISymbolizer
{
    /// <summary>
    /// Gets the symbol corresponding to the specified syntax node that
    /// declares a property or member accessor.
    /// </summary>
    /// <param name="node">
    /// The accessor declaration.
    /// </param>
    /// <returns>
    /// The symbol representing the method.
    /// </returns>
    IMethodSymbol? ToSymbol(AccessorDeclarationSyntax node);

    /// <summary>
    /// Gest the method symbol corresponding to the specified base method
    /// declaration syntax.
    /// </summary>
    /// <param name="node">
    /// The method declaration syntax.
    /// </param>
    /// <returns>
    /// The symbol representing the method.
    /// </returns>
    IMethodSymbol? ToSymbol(BaseMethodDeclarationSyntax node);

    /// <summary>
    /// Gets the symbol for the exception variable corresponding to the
    /// specified catch declaration.
    /// </summary>
    /// <param name="node">
    /// The catch declaration.
    /// </param>
    /// <returns>
    /// The symbol representing the exception variable.
    /// </returns>
    ILocalSymbol? ToSymbol(CatchDeclarationSyntax node);

    /// <summary>
    /// Gets the symbol for the iteration variable corresponding to the
    /// specified foreach statement.
    /// </summary>
    /// <param name="node">
    /// The foreach statement.
    /// </param>
    /// <returns>
    /// The symbol representing the iteration variable of the foreach
    /// statement.
    /// </returns>
    ILocalSymbol? ToSymbol(ForEachStatementSyntax node);

    /// <summary>
    /// Gets the symbol corresponding to the specified type parameter
    /// declaration (field or method).
    /// </summary>
    /// <param name="node">
    /// The type parameter declaration.
    /// </param>
    /// <returns>
    /// The symbol representing the type parameter.
    /// </returns>
    ITypeParameterSymbol? ToSymbol(TypeParameterSyntax node);

    /// <summary>
    /// Gets the symbol corresponding to the specified parameter declaration
    /// syntax node.
    /// </summary>
    /// <param name="node">
    /// The parameter declaration.
    /// </param>
    /// <returns>
    /// The symbol representing the parameter.
    /// </returns>
    IParameterSymbol? ToSymbol(ParameterSyntax node);

    /// <summary>
    /// Gets the symbol for the alias that was introduced corresponding to the
    /// specified extern alias declaration.
    /// </summary>
    /// <param name="node">
    /// The extern alias declaration.
    /// </param>
    /// <returns>
    /// The symbol representing the alias.
    /// </returns>
    IAliasSymbol? ToSymbol(ExternAliasDirectiveSyntax node);

    /// <summary>
    /// Gets the symbol for the using alias that was introduced corresponding
    /// to the specified using declaration.
    /// </summary>
    /// <param name="node">
    /// The using declaration.
    /// </param>
    /// <returns>
    /// The symbol representing the alias.
    /// </returns>
    IAliasSymbol? ToSymbol(UsingDirectiveSyntax node);

    /// <summary>
    /// Gets the namespace symbol for the declaration assembly corresponding to
    /// the specified namespace declaration syntax node.
    /// </summary>
    /// <param name="node">
    /// The namespace declaration.
    /// </param>
    /// <returns>
    /// The symbol representing the namespace.
    /// </returns>
    INamespaceSymbol? ToSymbol(NamespaceDeclarationSyntax node);

    /// <summary>
    /// Gets the type symbol corresponding to the specified type declaration.
    /// </summary>
    /// <param name="node">
    /// The type declaration.
    /// </param>
    /// <returns>
    /// The symbol representing the type.
    /// </returns>
    INamedTypeSymbol? ToSymbol(BaseTypeDeclarationSyntax node);

    /// <summary>
    /// Gets the type symbol corresponding to the specified delegate
    /// declaration.
    /// </summary>
    /// <param name="node">
    /// The delegate declaration.
    /// </param>
    /// <returns>
    /// The symbol representing the type.
    /// </returns>
    INamedTypeSymbol? ToSymbol(DelegateDeclarationSyntax node);

    /// <summary>
    /// Gets the anonymous object type symbol corresponding to the specified
    /// syntax node of anonymous object creation expression.
    /// </summary>
    /// <param name="node">
    /// The anonymous object creation expression.
    /// </param>
    /// <returns>
    /// The symbol representing the anonymous object type.
    /// </returns>
    INamedTypeSymbol? ToSymbol(
        AnonymousObjectCreationExpressionSyntax node);

    /// <summary>
    /// Gets the tuple type symbol corresponding to the syntax node of tuple
    /// expression.
    /// </summary>
    /// <param name="node">
    /// The tuple expression.
    /// </param>
    /// <returns>
    /// The symbol representing the tuple type.
    /// </returns>
    INamedTypeSymbol? ToSymbol(TupleExpressionSyntax node);

    /// <summary>
    /// Gets the field symbol corresponding to the enum member declaration.
    /// </summary>
    /// <param name="node">
    /// The enum member declaration.
    /// </param>
    /// <returns>
    /// The symbol representing the corresponding field.
    /// </returns>
    IFieldSymbol? ToSymbol(EnumMemberDeclarationSyntax node);

    /// <summary>
    /// Gets the declared symbol corresponding to the syntax node that declares
    /// a property.
    /// </summary>
    /// <param name="node">
    /// The property declaration.
    /// </param>
    /// <returns>
    /// The symbol representing the property.
    /// </returns>
    IPropertySymbol? ToSymbol(PropertyDeclarationSyntax node);

    /// <summary>
    /// Gets the declared symbol corresponding to the specified syntax node
    /// that declares an indexer.
    /// </summary>
    /// <param name="node">
    /// The indexer declaration.
    /// </param>
    /// <returns>
    /// The symbol representing the indexer.
    /// </returns>
    IPropertySymbol? ToSymbol(IndexerDeclarationSyntax node);

    /// <summary>
    /// Gets the anonymous object property symbol corresponding to the
    /// specified syntax node of anonymous object creation initializer.
    /// </summary>
    /// <param name="node">
    /// The anonymous object creation initializer.
    /// </param>
    /// <returns>
    /// The symbol representing the anonymous object creation initializer.
    /// </returns>
    IPropertySymbol? ToSymbol(AnonymousObjectMemberDeclaratorSyntax node);

    /// <summary>
    /// Gets the label symbol corresponding to the specified switch label
    /// syntax.
    /// </summary>
    /// <param name="node">
    /// The switch label.
    /// </param>
    /// <returns>
    /// The symbol representing the label.
    /// </returns>
    ILabelSymbol? ToSymbol(SwitchLabelSyntax node);

    /// <summary>
    /// Gets the label symbol corresponding to the specified labeled statement
    /// syntax.
    /// </summary>
    /// <param name="node">
    /// The labeled statement.
    /// </param>
    /// <returns>
    /// The symbol representing the labeled statement.
    /// </returns>
    ILabelSymbol? ToSymbol(LabeledStatementSyntax node);

    /// <summary>
    /// Gets the event symbol corresponding to the specified syntax node that
    /// declares a (custom) event.
    /// </summary>
    /// <param name="node">
    /// The event declaration.
    /// </param>
    /// <returns>
    /// The symbol representing the event.
    /// </returns>
    IEventSymbol? ToSymbol(EventDeclarationSyntax node);

    /// <summary>
    /// Gets the query range variable declared in the specified join into
    /// clause.
    /// </summary>
    /// <param name="node">
    /// The join into clause.
    /// </param>
    /// <returns>
    /// The symbol representing the query range variable.
    /// </returns>
    IRangeVariableSymbol? ToSymbol(JoinIntoClauseSyntax node);

    /// <summary>
    /// Gets the query range variable declared in the specified query clause.
    /// </summary>
    /// <param name="node">
    /// The query clause.
    /// </param>
    /// <returns>
    /// The symbol representing the query range variable.
    /// </returns>
    IRangeVariableSymbol? ToSymbol(QueryClauseSyntax node);

    /// <summary>
    /// Gets the query range variable declared in the specified query
    /// continuation clause.
    /// </summary>
    /// <param name="node">
    /// The query continuation clause.
    /// </param>
    /// <returns>
    /// The symbol representing the query range variable.
    /// </returns>
    IRangeVariableSymbol? ToSymbol(QueryContinuationSyntax node);

    /// <summary>
    /// Gets the symbol corresponding to the specified variable declarator.
    /// </summary>
    /// <param name="node">
    /// The variable declarator.
    /// </param>
    /// <returns>
    /// The symbol representing the variable.
    /// </returns>
    ISymbol? ToSymbol(VariableDeclaratorSyntax node);
}
