namespace Roastery;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The map that has values through the <see cref="WeakReference{V}"/>.
/// </summary>
/// <typeparam name="K">
/// The type of the key.
/// </typeparam>
/// <typeparam name="V">
/// The type of the value.
/// </typeparam>
public sealed class WeakValueMap<K, V>
    where K : notnull
    where V : class
{
    private readonly Dictionary<K, WeakReference<V>> map = [];

    /// <summary>
    /// Clear the keys whose values have been unavailable.
    /// </summary>
    public void ClearGabageKeys()
    {
        var gabageKeys = map.Where(p => !p.Value.TryGetTarget(out var _))
            .Select(p => p.Key)
            .ToList();
        foreach (var key in gabageKeys)
        {
            map.Remove(key);
        }
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">
    /// The key of the value to get.
    /// </param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the
    /// specified key, if the key is found; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if this contains an element with the specified key;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetValue(K key, out V? value)
    {
        ClearGabageKeys();
        if (map.TryGetValue(key, out var weakRef)
            && weakRef.TryGetTarget(out value))
        {
            return true;
        }
        value = null;
        return false;
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">
    /// The key of the value to get.
    /// </param>
    /// <returns>
    /// The value associated with the specified key, if the key is found;
    /// otherwise, <c>null</c>.
    /// </returns>
    public V? Get(K key)
    {
        ClearGabageKeys();
        return map.TryGetValue(key, out var weakRef)
               && weakRef.TryGetTarget(out var value)
            ? value
            : null;
    }

    /// <summary>
    /// Sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">
    /// The key of the value to set.
    /// </param>
    /// <param name="value">
    /// The value associated with the specified key.
    /// </param>
    public void Put(K key, V value)
    {
        map[key] = new WeakReference<V>(value);
    }
}
