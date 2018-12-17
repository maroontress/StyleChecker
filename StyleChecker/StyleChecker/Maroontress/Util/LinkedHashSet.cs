namespace Maroontress.Util
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Hash table and linked list implementation of the <see cref="ISet{T}"/>
    /// interface, with predictable iteration order.
    /// </summary>
    /// <remarks>
    /// This implementation differs from <see cref="HashSet{T}"/> in that it
    /// maintains a doubly-linked list running through all of its entries. This
    /// linked list defines the iteration ordering, which is the order in which
    /// elements were inserted into the set (<i>insertion-order</i>). Note that
    /// insertion order is <i>not</i> affected if an element is re-inserted
    /// into the set. (An element <c>e</c> is reinserted into a set <c>s</c> if
    /// <c>s.Add(e)</c> is invoked when <c>s.Contains(e)</c> would return
    /// <c>true</c> immediately prior to the invocation.)
    /// </remarks>
    /// <typeparam name="T">
    /// The type of elements maintained by this set.
    /// </typeparam>
    public sealed class LinkedHashSet<T> : ISet<T>
    {
        private readonly Dictionary<T, LinkedListNode<T>> map
            = new Dictionary<T, LinkedListNode<T>>();

        private readonly LinkedList<T> list = new LinkedList<T>();

        private readonly Func<ISet<T>> newKeySet;

        private Func<ISet<T>> keySet;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedHashSet{T}"/>
        /// class.
        /// </summary>
        public LinkedHashSet()
        {
            newKeySet = () =>
            {
                var set = new HashSet<T>(map.Keys);
                keySet = () => set;
                return set;
            };
            keySet = newKeySet;
        }

        /// <inheritdoc/>
        public int Count => list.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public bool Add(T item)
        {
            if (map.ContainsKey(item))
            {
                return false;
            }
            var listNode = list.AddLast(item);
            map[item] = listNode;
            keySet = newKeySet;
            return true;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            map.Clear();
            list.Clear();
            keySet = newKeySet;
        }

        /// <inheritdoc/>
        public bool Contains(T item) => map.ContainsKey(item);

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
            => list.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var e in other)
            {
                Remove(e);
            }
            keySet = newKeySet;
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

        /// <inheritdoc/>
        public void IntersectWith(IEnumerable<T> other)
        {
            var set = new HashSet<T>(other);
            var nodes = map.Where(pair => !set.Contains(pair.Key))
                .Select(p => p.Value);
            foreach (var n in nodes)
            {
                list.Remove(n);
            }
            var node = list.First;
            map.Clear();
            while (node != null)
            {
                map[node.Value] = node;
                node = node.Next;
            }
            keySet = newKeySet;
        }

        /// <inheritdoc/>
        public bool IsProperSubsetOf(IEnumerable<T> other)
            => keySet().IsProperSubsetOf(other);

        /// <inheritdoc/>
        public bool IsProperSupersetOf(IEnumerable<T> other)
            => keySet().IsProperSupersetOf(other);

        /// <inheritdoc/>
        public bool IsSubsetOf(IEnumerable<T> other)
            => keySet().IsSubsetOf(other);

        /// <inheritdoc/>
        public bool IsSupersetOf(IEnumerable<T> other)
            => keySet().IsSupersetOf(other);

        /// <inheritdoc/>
        public bool Overlaps(IEnumerable<T> other)
            => keySet().Overlaps(other);

        /// <inheritdoc/>
        public bool Remove(T node)
        {
            if (!map.TryGetValue(node, out var listNode))
            {
                return false;
            }
            list.Remove(listNode);
            map.Remove(node);
            keySet = newKeySet;
            return true;
        }

        /// <inheritdoc/>
        public bool SetEquals(IEnumerable<T> other)
            => keySet().SetEquals(other);

        /// <inheritdoc/>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            foreach (var e in other)
            {
                if (!Remove(e))
                {
                    Add(e);
                }
            }
            keySet = newKeySet;
        }

        /// <inheritdoc/>
        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var e in other)
            {
                Add(e);
            }
            keySet = newKeySet;
        }

        /// <inheritdoc/>
        void ICollection<T>.Add(T item) => Add(item);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
