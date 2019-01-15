namespace Maroontress.Oxbind.Impl
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// The map of a key to the <see cref="Reflector{T}"/> object.
    /// </summary>
    /// <typeparam name="K">
    /// The type of a key.
    /// </typeparam>
    /// <typeparam name="V">
    /// The type of a value of the <see cref="Reflector{T}"/>.
    /// </typeparam>
    public abstract class ReflectorMap<K, V> : Dictionary<K, Reflector<V>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectorMap{K, V}"/>
        /// class.
        /// </summary>
        protected ReflectorMap()
        {
        }

        /// <summary>
        /// Gets the new <see cref="Injector"/> that injects a value to the
        /// specified field.
        /// </summary>
        /// <param name="info">
        /// The field information associated with the field where the value is
        /// injected.
        /// </param>
        /// <returns>
        /// The new <see cref="Injector"/>.
        /// </returns>
        protected static Injector ToInjector(FieldInfo info)
            => (i, v) => info.SetValue(i, v);

        /// <summary>
        /// Gets the new <see cref="Injector"/> that injects a value with the
        /// specified method.
        /// </summary>
        /// <param name="info">
        /// The method information associated with the method which the value
        /// is injected with.
        /// </param>
        /// <returns>
        /// The new <see cref="Injector"/>.
        /// </returns>
        protected static Injector ToInjector(MethodInfo info)
            => (i, v) => info.Invoke(i, new object[] { v });

        /// <summary>
        /// Associates the <see cref="Reflector{T}"/> object with the specified
        /// key in this map.
        /// </summary>
        /// <remarks>
        /// No <see cref="Reflector{T}"/> must be associated with the specified
        /// key on ahead.
        /// </remarks>
        /// <param name="key">
        /// A key of the map.
        /// </param>
        /// <param name="reflector">
        /// The reflector associated with the key.
        /// </param>
        protected void Put(K key, Reflector<V> reflector)
        {
            this[key] = reflector;
        }
    }
}
