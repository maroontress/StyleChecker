namespace Maroontress.Extensions
{
    /// <summary>
    /// Provides the utility methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns another string if this is empty.
        /// </summary>
        /// <param name="self">
        /// the string instance.
        /// </param>
        /// <param name="other">
        /// the instance to be returned if <paramref name="self"/> is empty.
        /// </param>
        /// <returns>
        /// <paramref name="self"/> if its length is not zero,
        /// otherwise <paramref name="other"/>.
        /// </returns>
        public static string OrElseIfEmpty(this string self, string other)
        {
            return (self.Length is 0)
                ? other
                : self;
        }
    }
}
