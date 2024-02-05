namespace Maroontress.Extensions;

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
    /// non-null references which this <see cref="IEnumerable{T}"/>
    /// instance contains.
    /// </summary>
    /// <typeparam name="T">
    /// The reference type of elements.
    /// </typeparam>
    /// <param name="list">
    /// The <see cref="IEnumerable{T}"/> instance containing references and
    /// <c>null</c>s.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{T}"/> instance containing only non-null
    /// references.
    /// </returns>
    public static IEnumerable<T> FilterNonNullReference<T>(
            this IEnumerable<T?> list)
        where T : class
    {
        return list.OfType<T>();
    }

    /// <summary>
    /// Gets a new <see cref="IEnumerable{T}"/> instance containing the
    /// non-null values which this <see cref="IEnumerable{T}"/> instance
    /// contains.
    /// </summary>
    /// <typeparam name="T">
    /// The value type of elements.
    /// </typeparam>
    /// <param name="list">
    /// The <see cref="IEnumerable{T}"/> instance containing values and
    /// <c>null</c>s.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable{T}"/> instance containing only non-null
    /// values.
    /// </returns>
    public static IEnumerable<T> FilterNonNullValue<T>(
            this IEnumerable<T?> list)
        where T : struct
    {
        return list.SelectMany(
            i => i.HasValue ? new[] { i.Value } : Array.Empty<T>());
    }

    /// <summary>
    /// Gets the new <see cref="IEnumerable{T}"/> instance, which provides
    /// the tuples containing the zero-based index number and each element
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

    private class WithIndexImpl<T> : IEnumerable<(int Index, T Value)>
    {
        public WithIndexImpl(IEnumerable<T> all)
        {
            All = all;
        }

        public IEnumerable<T> All { get; }

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
