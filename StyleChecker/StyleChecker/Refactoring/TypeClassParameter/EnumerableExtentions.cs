namespace StyleChecker.Refactoring.TypeClassParameter
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Provides utility methods for a <see cref="IEnumerable"/>.
    /// </summary>
    public static class EnumerableExtentions
    {
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
        public static IEnumerable<(int index, T value)>
            WithIndex<T>(this IEnumerable<T> all)
        {
            return new WithIndexImpl<T>(all);
        }

        private class WithIndexImpl<T> : IEnumerable<(int, T)>
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

            IEnumerator<(int, T)> IEnumerable<(int, T)>.GetEnumerator()
            {
                var n = All.GetEnumerator();
                for (var k = 0; n.MoveNext(); ++k)
                {
                    yield return (k, n.Current);
                }
            }
        }
    }
}
