namespace StyleChecker.Analyzers.Refactoring;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides utility methods for a <see cref="ITypeSymbol"/>.
/// </summary>
public static class TypeSymbols
{
    /// <summary>
    /// Gets the Fully Qualified Type Name (FQTN) of the specified type symbol.
    /// </summary>
    /// <param name="typeSymbol">
    /// The type symbol.
    /// </param>
    /// <returns>
    /// The FQTN of the specified type symbol, with array notation if it is an
    /// array type.
    /// </returns>
    public static string GetFullName(ITypeSymbol typeSymbol)
    {
        return typeSymbol is IArrayTypeSymbol arrayTypeSymbol
            ? GetFullName(arrayTypeSymbol.ElementType) + "[]"
            : typeSymbol.ContainingNamespace + "." + typeSymbol.Name;
    }

    /// <summary>
    /// Gets the Fully Qualified Type Name (FQTN) of the specified type symbol
    /// without nullability.
    /// </summary>
    /// <param name="typeSymbol">
    /// The type symbol.
    /// </param>
    /// <returns>
    /// The FQTN of the specified type symbol without nullability, with array
    /// notation if it is an array type.
    /// </returns>
    public static string GetFullNameWithoutNullability(ITypeSymbol typeSymbol)
        => GetFullName(typeSymbol.WithNullableAnnotation(
            NullableAnnotation.NotAnnotated));
}
