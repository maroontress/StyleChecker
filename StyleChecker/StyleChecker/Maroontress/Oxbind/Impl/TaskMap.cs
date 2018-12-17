namespace Maroontress.Oxbind.Impl
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// The map of a key to the <see cref="Task{T}"/> object.
    /// </summary>
    /// <typeparam name="K">
    /// The type of a key.
    /// </typeparam>
    /// <typeparam name="V">
    /// The type of a value of the <see cref="Task{T}"/>.
    /// </typeparam>
    public abstract class TaskMap<K, V> : Dictionary<K, Task<V>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskMap{K, V}"/>
        /// class.
        /// </summary>
        protected TaskMap()
        {
        }

        /// <summary>
        /// Associates the <see cref="Task{T}"/> object with the specified key
        /// in this map.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="Task{T}"/> object uses the specified field to
        /// dispatch a value to an instance, so that the field on the instance
        /// is set to the new value.
        /// </para>
        /// <para>
        /// No <see cref="Task{T}"/> must be associated with the specified key
        /// on ahead.
        /// </para>
        /// </remarks>
        /// <param name="key">
        /// A key of the map.
        /// </param>
        /// <param name="f">
        /// The field that the mapped <see cref="Task{T}"/> with the key uses
        /// to set a value.
        /// </param>
        protected void PutField(K key, FieldInfo f)
        {
            PutTask(key, (instance, value)
                => f.SetValue(instance, value));
        }

        /// <summary>
        /// Associates the <see cref="Task{T}"/> object with the specified key
        /// in this map.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="Task{T}"/> object uses the specified
        /// method to dispatch a value to an instance, so that the method on
        /// the instance is invoked with a parameter of the new value.
        /// </para>
        /// <para>
        /// No <see cref="Task{T}"/> must be associated with the specified key
        /// on ahead.
        /// </para>
        /// </remarks>
        /// <param name="key">
        /// A key of the map.
        /// </param>
        /// <param name="m">
        /// The method that the mapped <see cref="Task{T}"/> with the key
        /// invokes with a parameter of the value.
        /// </param>
        protected void PutMethod(K key, MethodInfo m)
        {
            PutTask(key, (instance, value)
                => m.Invoke(instance, new object[] { value }));
        }

        /// <summary>
        /// Associates the <see cref="Task{T}"/> object with the specified key
        /// in this map.
        /// </summary>
        /// <param name="key">
        /// A key of the map.
        /// </param>
        /// <param name="value">
        /// A <see cref="Task{T}"/> object associated with the key.
        /// </param>
        private void PutTask(K key, Task<V> value)
        {
            this[key] = value;
        }
    }
}
