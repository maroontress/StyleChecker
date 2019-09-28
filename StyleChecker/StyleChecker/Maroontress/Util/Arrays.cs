namespace Maroontress.Util
{
    /// <summary>
    /// Provides the utility methods for arrays.
    /// </summary>
    public static class Arrays
    {
        /// <summary>
        /// Returns a new array containing the specified objects.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the element.
        /// </typeparam>
        /// <param name="elements">
        /// Elements that the array contains.
        /// </param>
        /// <returns>
        /// A new array containing the specified objects.
        /// </returns>
        public static T[] Of<T>(params T[] elements)
        {
            return elements;
        }
    }
}
