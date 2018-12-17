namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Maroontress.Util;

    /// <summary>
    /// The map of a <c>class</c> object annotated with <see
    /// cref="ForElementAttribute"/> to the <see cref="Task{T}"/> object that
    /// dispatches an <c>object</c> of the class to an instance.
    /// </summary>
    public sealed class ChildTaskMap : TaskMap<Type, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChildTaskMap"/> class.
        /// </summary>
        /// <param name="clazz">
        /// The class annotated with <see cref="ForChildAttribute"/> and/or
        /// <see cref="FromChildAttribute"/>.
        /// </param>
        private ChildTaskMap(Type clazz)
        {
            Scan(
                Classes.GetInstanceFields<ForChildAttribute>(clazz),
                f => f.FieldType,
                PutField);
            Scan(
                Classes.GetInstanceMethods<FromChildAttribute>(clazz),
                m => GetElementClass(m),
                PutMethod);
        }

        /// <summary>
        /// Returns a new unmodifiable map of a <c>class</c> object to the <see
        /// cref="Task{T}"/> object.
        /// </summary>
        /// <param name="clazz">
        /// The class that has fields annotated with <see
        /// cref="ForChildAttribute"/> and/or methods annotated with <see
        /// cref="FromChildAttribute"/>.
        /// </param>
        /// <returns>
        /// A new unmodifiable map. Each key in the map is the <c>class</c>
        /// object annotated with <see cref="ForElementAttribute"/>. The value
        /// associated with the key is the <see cref="Task{T}"/> object that
        /// dispatches the object whose class is the key to the instance of
        /// <paramref name="clazz"/> class.
        /// </returns>
        public static IReadOnlyDictionary<Type, Task<object>> Of(Type clazz)
        {
            return new ChildTaskMap(clazz);
        }

        /// <summary>
        /// Gets the type of each <see cref="MemberInfo"/> (that is either
        /// <see cref="FieldInfo"/> or <see cref="MethodInfo"/> in the
        /// specified <see cref="IEnumerable{T}"/> with the specified function,
        /// and associates the type with a <see cref="Task{T}"/> object
        /// using the specified <see cref="Action"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of <see cref="FieldInfo"/> or <see cref="MethodInfo"/>.
        /// </typeparam>
        /// <param name="all">
        /// The <see cref="MemberInfo"/> objects.
        /// </param>
        /// <param name="getKey">
        /// The function mapping of a <see cref="MemberInfo"/> to the type.
        /// </param>
        /// <param name="putTask">
        /// <see cref="TaskMap{K,V}.PutField(K, FieldInfo)"/> or
        /// <see cref="TaskMap{K,V}.PutMethod(K, MethodInfo)"/>.
        /// </param>
        private static void Scan<T>(
                    IEnumerable<T> all,
                    Func<T, Type> getKey,
                    Action<Type, T> putTask)
                    where T : MemberInfo
        {
            foreach (var m in all)
            {
                var clazz = getKey(m);
                putTask(clazz, m);
            }
        }

        /// <summary>
        /// Returns the type of the single parameter of the specified method.
        /// </summary>
        /// <param name="m">
        /// The <see cref="MethodInfo"/> object that is of the instance method,
        /// whose return type is {@code void}, and that has a single parameter.
        /// </param>
        /// <returns>
        /// The type of the single parameter.
        /// </returns>
        private Type GetElementClass(MethodInfo m)
        {
            var returnType = m.ReturnType;
            if (returnType != typeof(void))
            {
                throw new BindingException(returnType.ToString());
            }
            var paramTypes = m.GetParameters()
                .Select(p => p.ParameterType);
            if (paramTypes.Count() != 1)
            {
                throw new BindingException(paramTypes.ToString());
            }
            return paramTypes.First();
        }
    }
}
