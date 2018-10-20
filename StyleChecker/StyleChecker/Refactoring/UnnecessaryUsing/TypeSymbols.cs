namespace StyleChecker.Refactoring.UnnecessaryUsing
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Provides utility methods for a <see cref="ITypeSymbol"/>.
    /// </summary>
    public static class TypeSymbols
    {
        /// <summary>
        /// Gets the Fully Qualified Type Name (FQTN) of the specified type
        /// symbol.
        /// </summary>
        /// <param name="typeSymbol">
        /// The type symbol.
        /// </param>
        /// <returns>
        /// The FQTN of the specified type symbol, with array notation
        /// if it is an array type.
        /// </returns>
        public static string GetFullName(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                return GetFullName(arrayTypeSymbol.ElementType) + "[]";
            }
            var typeFullName = typeSymbol.ContainingNamespace
                + "."
                + typeSymbol.Name;
            return typeFullName;
        }
    }
}
