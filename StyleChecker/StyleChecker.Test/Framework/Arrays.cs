namespace StyleChecker.Test.Framework
{
    /// <summary>
    /// Provides the utility methods for arrays.
    /// </summary>
    public static class Arrays
    {
        /// <summary>
        /// Returns a new array containing only the specified object.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the element.
        /// </typeparam>
        /// <param name="element">
        /// An element that the array contains.
        /// </param>
        /// <returns>
        /// A new array containing only the specified object.
        /// </returns>
        public static T[] Singleton<T>(T element)
        {
            return new T[] { element };
        }
    }
}
