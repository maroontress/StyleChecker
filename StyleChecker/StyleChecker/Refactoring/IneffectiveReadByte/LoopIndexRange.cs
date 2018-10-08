namespace StyleChecker.Refactoring.IneffectiveReadByte
{
    using Microsoft.CodeAnalysis;

    public sealed class LoopIndexRange
    {
        public LoopIndexRange(ISymbol symbol, int start, int end)
        {
            Symbol = symbol;
            Start = start;
            End = end;
        }

        public ISymbol Symbol{ get; }

        public int Start { get; }

        public int End { get; }
    }
}
