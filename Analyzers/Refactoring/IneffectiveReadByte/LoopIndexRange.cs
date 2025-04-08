namespace Analyzers.Refactoring.IneffectiveReadByte;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents properties of the index and its range in a constant-incremental
/// loop.
/// </summary>
/// <param name="symbol">
/// The symbol of the index variable.
/// </param>
/// <param name="start">
/// The start index.
/// </param>
/// <param name="end">
/// The end index.
/// </param>
public sealed class LoopIndexRange(ISymbol symbol, int start, int end)
{
    /// <summary>
    /// Gets the symbol of the index variable.
    /// </summary>
    public ISymbol Symbol { get; } = symbol;

    /// <summary>
    /// Gets the start index.
    /// </summary>
    public int Start { get; } = start;

    /// <summary>
    /// Gets the end index.
    /// </summary>
    public int End { get; } = end;
}
