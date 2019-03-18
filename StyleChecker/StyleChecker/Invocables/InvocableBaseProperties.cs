namespace StyleChecker.Invocables
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// The abstract properties of <see cref="BaseMethodDeclarationSyntax"/>
    /// and <see cref="LocalFunctionStatementSyntax"/> objects.
    /// </summary>
    public abstract class InvocableBaseProperties
    {
        /// <summary>
        /// Gets the parameter list.
        /// </summary>
        public virtual ParameterListSyntax ParameterList { get; }

        /// <summary>
        /// Gets the body.
        /// </summary>
        public virtual BlockSyntax Body { get; }

        /// <summary>
        /// Gets the expression body.
        /// </summary>
        public virtual ArrowExpressionClauseSyntax ExpressionBody { get; }

        /// <summary>
        /// Gets the wrapped node.
        /// </summary>
        public virtual SyntaxNode Node { get; }

        /// <summary>
        /// Gets the modifiers.
        /// </summary>
        public virtual SyntaxTokenList Modifiers { get; }
    }
}
