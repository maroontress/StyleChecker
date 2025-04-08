namespace Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

/// <summary>
/// Provides extension methods for <c>ISymbol</c> and the utilities.
/// </summary>
public static class Symbols
{
    /// <summary>
    /// Determines whether two symbols are equal.
    /// </summary>
    /// <param name="first">
    /// The first symbol.
    /// </param>
    /// <param name="second">
    /// The second symbol.
    /// </param>
    /// <returns>
    /// <c>true</c> if the symbols are equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool AreEqual(ISymbol first, ISymbol second)
        => SymbolEqualityComparer.Default.Equals(first, second);

    /// <summary>
    /// Converts the specified source collection to a read-only set.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements in the collection.
    /// </typeparam>
    /// <param name="source">
    /// The source collection.
    /// </param>
    /// <returns>
    /// A read-only set containing the elements from the source collection.
    /// </returns>
    public static IImmutableSet<T> ToRigidSet<T>(
            this IEnumerable<T> source)
        where T : ISymbol
    {
        return source.ToImmutableHashSet(new Comparator<T>());
    }

    /// <summary>
    /// Converts the specified source collection to a read-only dictionary.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements in the collection.
    /// </typeparam>
    /// <typeparam name="K">
    /// The type of keys in the dictionary.
    /// </typeparam>
    /// <typeparam name="V">
    /// The type of values in the dictionary.
    /// </typeparam>
    /// <param name="source">
    /// The source collection.
    /// </param>
    /// <param name="keySelector">
    /// A function to extract the key from each element.
    /// </param>
    /// <param name="valueSelector">
    /// A function to extract the value from each element.
    /// </param>
    /// <returns>
    /// A read-only dictionary containing the elements from the source
    /// collection.
    /// </returns>
    public static IImmutableDictionary<K, V> ToRigidMap<T, K, V>(
            this IEnumerable<T> source,
            Func<T, K> keySelector,
            Func<T, V> valueSelector)
        where K : ISymbol
    {
        return source.ToImmutableDictionary(
            keySelector, valueSelector, new Comparator<K>());
    }

    private class Comparator<T> : IEqualityComparer<T>
        where T : ISymbol
    {
        private static readonly SymbolEqualityComparer Comparer
            = SymbolEqualityComparer.Default;

        bool IEqualityComparer<T>.Equals(T x, T y)
        {
            return Comparer.Equals(x, y);
        }

        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            return Comparer.GetHashCode(obj);
        }
    }
}
