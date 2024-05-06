namespace Maroontress.Util;

using System.Collections.Generic;

/// <summary>
/// Provides the utility methods for <see cref="IEnumerable{T}"/>.
/// </summary>
public static class Enumerables
{
    /// <summary>
    /// Returns a new <see cref="IEnumerable{T}"/> object containing the
    /// specified objects.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the element.
    /// </typeparam>
    /// <param name="elements">
    /// Elements that the <see cref="IEnumerable{T}"/> object contains.
    /// </param>
    /// <returns>
    /// A new <see cref="IEnumerable{T}"/> object containing the specified
    /// objects.
    /// </returns>
    public static IEnumerable<T> Of<T>(params T[] elements)
    {
        return elements;
    }
}
