namespace StyleChecker.Refactoring.IneffectiveReadByte
{
    using Microsoft.CodeAnalysis;

    public static partial class ExpressionStatements
    {
        public sealed class ArrayAccess
        {
            public ArrayAccess(ISymbol array, ISymbol index)
            {
                Array = array;
                Index = index;
            }

            public ISymbol Array { get; }

            public ISymbol Index { get; }
        }
    }
}
