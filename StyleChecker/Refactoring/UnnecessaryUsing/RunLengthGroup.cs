namespace StyleChecker.Refactoring.UnnecessaryUsing;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// Represents a static class that provides a method for grouping elements in a
/// sequence based on a specified key.
/// </summary>
public static class RunLengthGroup
{
    /// <summary>
    /// Groups the elements of a sequence based on a specified key.
    /// </summary>
    /// <typeparam name="K">
    /// The type of the key.
    /// </typeparam>
    /// <typeparam name="T">
    /// The type of the elements in the sequence.
    /// </typeparam>
    /// <param name="source">
    /// The sequence of elements to group.
    /// </param>
    /// <param name="classify">
    /// A function that returns the key for each element.
    /// </param>
    /// <returns>
    /// An enumerable collection of groups, where each group contains a key and
    /// a collection of elements with that key.
    /// </returns>
    public static IEnumerable<IGrouping<K, T>> RunLengthGroupBy<K, T>(
            this IEnumerable<T> source, Func<T, K> classify)
        where K : notnull
    {
        var list = ImmutableArray.CreateBuilder<T>();
        var e = source.GetEnumerator();
        if (!e.MoveNext())
        {
            yield break;
        }
        var firstItem = e.Current;
        var state = classify(firstItem);
        list.Add(firstItem);
        while (e.MoveNext())
        {
            var item = e.Current;
            var nextState = classify(item);
            if (nextState.Equals(state))
            {
                list.Add(item);
                continue;
            }
            yield return new Grouping<K, T>(state, list.ToImmutable());
            state = nextState;
            list.Clear();
            list.Add(item);
        }
        if (list.Count > 0)
        {
            yield return new Grouping<K, T>(state, list.ToImmutable());
        }
    }

    private sealed class Grouping<K, T>(K key, IEnumerable<T> all)
        : IGrouping<K, T>
    {
        public K Key => key;

        public IEnumerator<T> GetEnumerator() => all.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
