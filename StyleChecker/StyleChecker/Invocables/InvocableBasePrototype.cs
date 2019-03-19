namespace StyleChecker.Invocables
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// The abstract factory methods of <see
    /// cref="BaseMethodDeclarationSyntax"/> and <see
    /// cref="LocalFunctionStatementSyntax"/> objects.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="InvocableBaseNodePod"/> class or its descendants.
    /// </typeparam>
    public interface InvocableBasePrototype<out T>
    {
        /// <summary>
        /// Gets a new <c>T</c> object with the specified <see
        /// cref="BlockSyntax"/> object.
        /// </summary>
        /// <param name="node">
        /// The <see cref="BlockSyntax"/> object.
        /// </param>
        /// <returns>
        /// The new <c>T</c> object.
        /// </returns>
        T With(BlockSyntax node);

        /// <summary>
        /// Gets a new <c>T</c> object with the specified <see
        /// cref="ParameterListSyntax"/> object.
        /// </summary>
        /// <param name="node">
        /// The <see cref="ParameterListSyntax"/> object.
        /// </param>
        /// <returns>
        /// The new <c>T</c> object.
        /// </returns>
        T With(ParameterListSyntax node);

        /// <summary>
        /// Gets a new <c>T</c> object with the specified <see
        /// cref="ArrowExpressionClauseSyntax"/> object.
        /// </summary>
        /// <param name="node">
        /// The <see cref="ArrowExpressionClauseSyntax"/> object.
        /// </param>
        /// <returns>
        /// The new <c>T</c> object.
        /// </returns>
        T With(ArrowExpressionClauseSyntax node);

        /// <summary>
        /// Gets a new <c>T</c> object with the specified <see
        /// cref="SyntaxToken"/> object representing a semicolon.
        /// </summary>
        /// <param name="node">
        /// The <see cref="SyntaxToken"/> object.
        /// </param>
        /// <returns>
        /// The new <c>T</c> object.
        /// </returns>
        T With(SyntaxToken node);
    }
}
