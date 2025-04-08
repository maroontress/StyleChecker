namespace Analyzers.Refactoring.IneffectiveReadByte;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents properties of the array access where the index is a variable.
/// </summary>
/// <param name="array">
/// The symbol of the array variable.
/// </param>
/// <param name="index">
/// The symbol of the index.
/// </param>
public sealed class ArrayAccess(ISymbol array, ISymbol index)
{
    /// <summary>
    /// Gets the symbol of the array variable.
    /// </summary>
    public ISymbol Array { get; } = array;

    /// <summary>
    /// Gets the symbol of the index.
    /// </summary>
    public ISymbol Index { get; } = index;
}
