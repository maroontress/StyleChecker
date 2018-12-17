namespace Maroontress.Util
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an array that is immutable; meaning it cannot be changed
    /// once it is created.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements stored in the array.
    /// </typeparam>
    public sealed class ImmutableArray<T> : IReadOnlyList<T>
    {
        /// <summary>
        /// The back-end array that represents this.
        /// </summary>
        private readonly T[] array;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableArray{T}"/>
        /// class.
        /// </summary>
        /// <param name="args">
        /// The array to be copied.
        /// </param>
        public ImmutableArray(T[] args)
        {
            array = (args.Length == 0)
                    ? Array.Empty<T>()
                    : args.Clone() as T[];
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>
        /// The number of elements in the collection.
        /// </value>
        public int Count => array.Length;

        /// <summary>
        /// Gets the element at the specified index in the read-only list.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to get.
        /// </param>
        /// <value>
        /// The element at the specified index in the read-only list.
        /// </value>
        public T this[int index] => array[index];

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            var n = array.Length;
            for (var k = 0; k < n; ++k)
            {
                yield return array[k];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
            => array.GetEnumerator();
    }
}
