namespace StyleChecker.Refactoring.IneffectiveReadByte
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents properties of the index and its range in a
    /// constant-incremental loop.
    /// </summary>
    public sealed class LoopIndexRange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoopIndexRange"/>
        /// class.
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
        public LoopIndexRange(ISymbol symbol, int start, int end)
        {
            Symbol = symbol;
            Start = start;
            End = end;
        }

        /// <summary>
        /// Gets the symbol of the index variable.
        /// </summary>
        public ISymbol Symbol { get; }

        /// <summary>
        /// Gets the start index.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the end index.
        /// </summary>
        public int End { get; }
    }
}
