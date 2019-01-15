namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Maroontress.Util;

    /// <summary>
    /// The map of a <c>class</c> object annotated with <see
    /// cref="ForElementAttribute"/> to the <see cref="Reflector{T}"/> object
    /// that dispatches an <c>object</c> of the class to an instance.
    /// </summary>
    public sealed class ChildReflectorMap : ReflectorMap<Type, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChildReflectorMap"/>
        /// class.
        /// </summary>
        /// <param name="clazz">
        /// The class annotated with <see cref="ForChildAttribute"/> and/or
        /// <see cref="FromChildAttribute"/>.
        /// </param>
        private ChildReflectorMap(Type clazz)
        {
            Scan(
                Classes.GetInstanceFields<ForChildAttribute>(clazz),
                f => f.FieldType,
                ToInjector,
                Put);
            Scan(
                Classes.GetInstanceMethods<FromChildAttribute>(clazz),
                GetElementClass,
                ToInjector,
                Put);
        }

        /// <summary>
        /// Returns a new unmodifiable map of a <c>class</c> object to the <see
        /// cref="Reflector{T}"/> object.
        /// </summary>
        /// <param name="clazz">
        /// The class that has fields annotated with <see
        /// cref="ForChildAttribute"/> and/or methods annotated with <see
        /// cref="FromChildAttribute"/>.
        /// </param>
        /// <returns>
        /// A new unmodifiable map. Each key in the map is the <c>class</c>
        /// object annotated with <see cref="ForElementAttribute"/>. The value
        /// associated with the key is the <see cref="Reflector{T}"/> object
        /// that dispatches the object whose class is the key to the instance
        /// of <paramref name="clazz"/> class.
        /// </returns>
        public static IReadOnlyDictionary<Type, Reflector<object>>
            Of(Type clazz)
        {
            return new ChildReflectorMap(clazz);
        }

        /// <summary>
        /// Gets the type of each <see cref="MemberInfo"/> (that is either
        /// <see cref="FieldInfo"/> or <see cref="MethodInfo"/> in the
        /// specified <see cref="IEnumerable{T}"/> with the specified function,
        /// and associates the type with a <see cref="Reflector{T}"/> object
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
        /// <param name="toInjector">
        /// The function that returns the injector associated with the
        /// specified type.
        /// </param>
        /// <param name="put">
        /// <see cref="ReflectorMap{K,V}.Put(K, Reflector{V})"/>.
        /// </param>
        private static void Scan<T>(
            IEnumerable<T> all,
            Func<T, Type> getKey,
            Func<T, Injector> toInjector,
            Action<Type, Reflector<object>> put)
            where T : MemberInfo
        {
            foreach (var m in all)
            {
                var type = getKey(m);
                var injector = toInjector(m);
                ObjectReflectors.Of(type, injector, (p, r) => put(p, r));
            }
        }

        /// <summary>
        /// Returns the type of the single parameter of the specified method.
        /// </summary>
        /// <param name="m">
        /// The <see cref="MethodInfo"/> object that is of the instance method,
        /// whose return type is <c>void</c>, and that has a single parameter.
        /// </param>
        /// <returns>
        /// The type of the single parameter.
        /// </returns>
        private Type GetElementClass(MethodInfo m)
        {
            var returnType = m.ReturnType;
            Debug.Assert(returnType.Equals(Types.Void), $"{returnType}");
            var paramTypes = m.GetParameters()
                .Select(p => p.ParameterType);
            var count = paramTypes.Count();
            Debug.Assert(count == 1, $"{count}");
            return paramTypes.First();
        }
    }
}
