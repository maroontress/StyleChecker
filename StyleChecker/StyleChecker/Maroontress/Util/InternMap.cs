namespace Maroontress.Util
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Provides canonical value objects corresponding to the key.
    /// </summary>
    /// <typeparam name="K">
    /// The type of the key.
    /// </typeparam>
    /// <typeparam name="V">
    /// The type of the value.
    /// </typeparam>
    public sealed class InternMap<K, V>
        where K : class
        where V : class
    {
        /// <summary>
        /// The map from a key to the value.
        /// </summary>
        private readonly ConcurrentDictionary<K, V> map
            = new ConcurrentDictionary<K, V>();

        /// <summary>
        /// Returns the canonical value object corresponding to the specified
        /// key object. If the canonical value object does not exist in the
        /// internal object pool, creates a new value object with the specified
        /// function.
        /// </summary>
        /// <remarks>
        /// The specified function can be called concurrently with the equal
        /// keys if the multiple threads call this method. However, the
        /// canonical value object that this method returns is only one.
        /// </remarks>
        /// <param name="key">
        /// The key object.
        /// </param>
        /// <param name="newValue">
        /// The function that returns a new value object corresponding to the
        /// specified argument.
        /// </param>
        /// <returns>
        /// The canonical value object.
        /// </returns>
        public V Intern(K key, Func<K, V> newValue)
        {
            return Intern(key, () => newValue(key));
        }

        /// <summary>
        /// Returns the canonical value object corresponding to the specified
        /// key object. If the canonical value object does not exist in the
        /// internal object pool, creates a new value object with the specified
        /// supplier.
        /// </summary>
        /// <remarks>
        /// The specified function can be called concurrently with the equal
        /// keys if the multiple threads call this method. However, the
        /// canonical value object that this method returns is only one.
        /// </remarks>
        /// <param name="key">
        /// The key object.
        /// </param>
        /// <param name="supplier">
        /// The supplier that returns a new value object corresponding to the
        /// specified key object.
        /// </param>
        /// <returns>
        /// The canonical value object.
        /// </returns>
        public V Intern(K key, Func<V> supplier)
        {
            if (map.TryGetValue(key, out var value))
            {
                return value;
            }
            value = supplier();
            var canonical = map.GetOrAdd(key, value);
            return (canonical == value) ? value : canonical;
        }
    }
}
