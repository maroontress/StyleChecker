namespace CodeDebt.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides utility methods for a <see cref="IEnumerable"/>.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Gets a new <see cref="IEnumerable{T}"/> instance containing the
    /// non-null references which this <see cref="IEnumerable{T}"/> instance
    /// contains.
    /// </summary>
    /// <typeparam name="T">
    /// The reference type of elements.
    /// </typeparam>
    /// <param name="source">
    /// The <see cref="IEnumerable{T}"/> instance containing references and
    /// <c>null</c>s.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{T}"/> instance containing only non-null
    /// references.
    /// </returns>
    public static IEnumerable<T> FilterNonNullReference<T>(
            this IEnumerable<T?> source)
        where T : class
    {
        return source.OfType<T>();
    }

    /// <summary>
    /// Gets a new <see cref="IEnumerable{T}"/> instance containing the
    /// non-null values which this <see cref="IEnumerable{T}"/> instance
    /// contains.
    /// </summary>
    /// <typeparam name="T">
    /// The value type of elements.
    /// </typeparam>
    /// <param name="source">
    /// The <see cref="IEnumerable{T}"/> instance containing values and
    /// <c>null</c>s.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{T}"/> instance containing only non-null
    /// values.
    /// </returns>
    public static IEnumerable<T> FilterNonNullValue<T>(
            this IEnumerable<T?> source)
        where T : struct
    {
        return source.SelectMany(i => i is {} v ? [v] : Array.Empty<T>());
    }

    /// <summary>
    /// Gets the first element of a sequence, or a null value if the sequence
    /// contains no elements.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the elements of source.
    /// </typeparam>
    /// <param name="source">
    /// The <see cref="IEnumerable{T}"/> to return the first element of.
    /// </param>
    /// <returns>
    /// <c>null</c> if source is empty; otherwise, the first element in source.
    /// </returns>
    public static T? FirstValue<T>(this IEnumerable<T> source)
        where T : struct
    {
        foreach (var first in source.Take(1))
        {
            return first;
        }
        return null;
    }

    /// <summary>
    /// Gets the new <see cref="IEnumerable{T}"/> instance, which provides the
    /// tuples containing the zero-based index number and each element
    /// contained in the specified <see cref="IEnumerable{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elemtents.
    /// </typeparam>
    /// <param name="all">
    /// The enumerable instance.
    /// </param>
    /// <returns>
    /// The new enumerable instance.
    /// </returns>
    public static IEnumerable<(int Index, T Value)>
        WithIndex<T>(this IEnumerable<T> all)
    {
        return new WithIndexImpl<T>(all);
    }

    private class WithIndexImpl<T>(IEnumerable<T> all)
        : IEnumerable<(int Index, T Value)>
    {
        public IEnumerable<T> All { get; } = all;

        public IEnumerator GetEnumerator()
        {
            var n = All.GetEnumerator();
            for (var k = 0; n.MoveNext(); ++k)
            {
                yield return (k, n.Current);
            }
        }

        IEnumerator<(int Index, T Value)>
            IEnumerable<(int Index, T Value)>.GetEnumerator()
        {
            var n = All.GetEnumerator();
            for (var k = 0; n.MoveNext(); ++k)
            {
                yield return (k, n.Current);
            }
        }
    }
}
