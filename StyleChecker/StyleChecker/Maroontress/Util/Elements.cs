namespace Maroontress.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The utility class for <see cref="ICollection{T}"/> objects and loop
    /// operations.
    /// </summary>
    public static class Elements
    {
        /// <summary>
        /// Performs an action for each <c>int</c> value that is sequential
        /// ordered from 0 (inclusive) to the specified value (exclusive) by an
        /// incremental step of 1.
        /// </summary>
        /// <param name="count">
        /// The exclusive upper bound.
        /// </param>
        /// <param name="consumer">
        /// The action to perform <paramref name="count"/> times with the
        /// argument 0, 1, ..., <paramref name="count"/> &#x2212; 1 (in order
        /// of increasing).
        /// </param>
        public static void ForEach(int count, Action<int> consumer)
        {
            for (var k = 0; k < count; ++k)
            {
                consumer(k);
            }
        }

        /// <summary>
        /// Accepts the specified consumer function if the specified collection
        /// is not empty.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the collection's element.
        /// </typeparam>
        /// <param name="all">
        /// The collection.
        /// </param>
        /// <param name="consumer">
        /// The consumer function to apply when the collection is not empty.
        /// </param>
        public static void IfNotEmpty<T>(
            IEnumerable<T> all, Action<IEnumerable<T>> consumer)
        {
            if (!all.Any())
            {
                return;
            }
            consumer(all);
        }

        /// <summary>
        /// Returns the relative complement of B in A (also called the
        /// set-theoretic difference of A and B), denoted by A \ B (or A
        /// &#x2212; B), is the set of all elements that are members of A but
        /// not members of B.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the element.
        /// </typeparam>
        /// <param name="a">
        /// The collection A.
        /// </param>
        /// <param name="b">
        /// The collection B.
        /// </param>
        /// <returns>
        /// The new <see cref="ISet{T}"/> object that has the relative
        /// complement of B in A.
        /// </returns>
        public static ISet<T> DifferenceOf<T>(
            IEnumerable<T> a, IEnumerable<T> b)
        {
            var set = new HashSet<T>(b);
            return new HashSet<T>(a.Where(e => !set.Contains(e)));
        }
    }
}
