namespace StyleChecker.Analyzers.Invocables;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// The abstract properties of <see cref="BaseMethodDeclarationSyntax"/> and
/// <see cref="LocalFunctionStatementSyntax"/> objects.
/// </summary>
public abstract class InvocableBaseProperties
{
    /// <summary>
    /// Gets the parameter list.
    /// </summary>
    public abstract ParameterListSyntax ParameterList { get; }

    /// <summary>
    /// Gets the body.
    /// </summary>
    public abstract BlockSyntax? Body { get; }

    /// <summary>
    /// Gets the expression body.
    /// </summary>
    public abstract ArrowExpressionClauseSyntax? ExpressionBody { get; }

    /// <summary>
    /// Gets the wrapped node.
    /// </summary>
    public abstract SyntaxNode Node { get; }

    /// <summary>
    /// Gets the modifiers.
    /// </summary>
    public abstract SyntaxTokenList Modifiers { get; }
}
