namespace StyleChecker.Invocables
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// The abstract properties of <see cref="MethodDeclarationSyntax"/>
    /// and <see cref="LocalFunctionStatementSyntax"/> objects.
    /// </summary>
    public abstract class InvocableProperties : InvocableBaseProperties
    {
        /// <summary>
        /// Gets the type parameter list.
        /// </summary>
        public virtual TypeParameterListSyntax TypeParameterList { get; }

        /// <summary>
        /// Gets the return type.
        /// </summary>
        public virtual TypeSyntax ReturnType { get; }
    }
}
