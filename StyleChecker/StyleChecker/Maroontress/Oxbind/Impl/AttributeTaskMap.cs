namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Maroontress.Util;

    /// <summary>
    /// The map of an attribute name to the <see cref="Task{T}"/> object
    /// that dispatches a <see cref="string"/> value to an instance.
    /// </summary>
    public sealed class AttributeTaskMap : TaskMap<string, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeTaskMap"/>
        /// class.
        /// </summary>
        /// <param name="clazz">
        /// The class annotated with <see cref="FromAttributeAttribute"/>
        /// and/or <see cref="ForAttributeAttribute"/>.
        /// </param>
        private AttributeTaskMap(Type clazz)
        {
            Scan(
                Classes.GetInstanceMethods<FromAttributeAttribute>(clazz),
                m => m.GetCustomAttribute<FromAttributeAttribute>().Name,
                PutMethod);
            Scan(
                Classes.GetInstanceFields<ForAttributeAttribute>(clazz),
                m => m.GetCustomAttribute<ForAttributeAttribute>().Name,
                PutField);
        }

        /// <summary>
        /// Returns a new unmodifiable map of an attribute name to the
        /// <cref cref="Task{T}"/> object.
        /// </summary>
        /// <param name="clazz">
        /// The class that has fields annotated with
        /// <see cref="ForAttributeAttribute"/>
        /// and/or methods annotated with <see cref="FromAttributeAttribute"/>.
        /// </param>
        /// <returns>
        /// A new unmodifiable map. Each key in the map is the attribute name
        /// specified with the argument of <see cref="FromAttributeAttribute"/>
        /// or <see cref="ForAttributeAttribute"/>. The value associated with
        /// the key is the <see cref="Task{T}"/> object that dispatches the
        /// attribute value to the instance of <paramref name="clazz"/> class.
        /// </returns>
        public static IReadOnlyDictionary<string, Task<string>> Of(Type clazz)
        {
            return new AttributeTaskMap(clazz);
        }

        /// <summary>
        /// Gets the attribute name of each member (that is either field or
        /// method) in the specified collection with the specified function,
        /// and associates the attribute name with a <see cref="Task{T}"/>
        /// object using the specified Action.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the field or method.
        /// </typeparam>
        /// <param name="all">
        /// The collection of the <c>Member</c>.
        /// </param>
        /// <param name="getAnnotation">
        /// The function mapping of a <c>Member</c> to the attribute name.
        /// </param>
        /// <param name="putTask">
        /// The action to inject the value to the field/method.
        /// </param>
        private static void Scan<T>(
            IEnumerable<T> all,
            Func<T, string> getAnnotation,
            Action<string, T> putTask)
            where T : MemberInfo
        {
            foreach (var m in all)
            {
                var name = getAnnotation(m);
                putTask(name, m);
            }
        }
    }
}
