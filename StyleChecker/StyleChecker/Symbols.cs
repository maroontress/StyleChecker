namespace StyleChecker
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Provides utility methods for <c>ISymbol</c>s.
    /// </summary>
    public static class Symbols
    {
        /// <summary>
        /// Gets whether the specified symbols are equal.
        /// </summary>
        /// <param name="s1">
        /// The symbol.
        /// </param>
        /// <param name="s2">
        /// The other symbol.
        /// </param>
        /// <returns>
        /// <c>true</c> if <paramref name="s1"/> is equal to <paramref name="s2"/>,
        /// otherwise <c>false</c>.
        /// </returns>
        public static bool AreEqual(ISymbol s1, ISymbol? s2)
        {
            return Equals(s1, s2);
        }
    }
}
