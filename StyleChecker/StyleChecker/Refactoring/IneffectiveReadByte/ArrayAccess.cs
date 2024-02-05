namespace StyleChecker.Refactoring.IneffectiveReadByte;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents properties of the array access where the index is a
/// variable.
/// </summary>
public sealed class ArrayAccess
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayAccess"/> class.
    /// </summary>
    /// <param name="array">
    /// The symbol of the array variable.
    /// </param>
    /// <param name="index">
    /// The symbol of the index.
    /// </param>
    public ArrayAccess(ISymbol array, ISymbol index)
    {
        Array = array;
        Index = index;
    }

    /// <summary>
    /// Gets the symbol of the array variable.
    /// </summary>
    public ISymbol Array { get; }

    /// <summary>
    /// Gets the symbol of the index.
    /// </summary>
    public ISymbol Index { get; }
}
