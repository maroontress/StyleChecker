namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using Maroontress.Util;

    /// <summary>
    /// The map of an attribute name to the <see cref="Reflector{T}"/> object
    /// that dispatches a <see cref="string"/> value to an instance.
    /// </summary>
    public sealed class AttributeReflectorMap
        : ReflectorMap<XmlQualifiedName, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="AttributeReflectorMap"/> class.
        /// </summary>
        /// <param name="clazz">
        /// The class annotated with <see cref="FromAttributeAttribute"/>
        /// and/or <see cref="ForAttributeAttribute"/>.
        /// </param>
        private AttributeReflectorMap(Type clazz)
        {
            Scan(
                Classes.GetInstanceMethods<FromAttributeAttribute>(clazz),
                m => m.GetCustomAttribute<FromAttributeAttribute>().QName,
                m => m.GetParameters().First().ParameterType,
                m => ToInjector(m),
                Put);
            Scan(
                Classes.GetInstanceFields<ForAttributeAttribute>(clazz),
                f => f.GetCustomAttribute<ForAttributeAttribute>().QName,
                f => f.FieldType,
                f => ToInjector(f),
                Put);
        }

        /// <summary>
        /// Returns a new unmodifiable map of an attribute name to the <cref
        /// cref="Reflector{T}"/> object.
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
        /// the key is the <see cref="Reflector{T}"/> object that dispatches
        /// the attribute value to the instance of <paramref name="clazz"/>
        /// class.
        /// </returns>
        public static IReadOnlyDictionary<XmlQualifiedName, Reflector<string>>
            Of(Type clazz)
        {
            return new AttributeReflectorMap(clazz);
        }

        /// <summary>
        /// Gets the attribute name of each member (that is either field or
        /// method) in the specified collection with the specified function,
        /// and associates the attribute name with a <see cref="Reflector{T}"/>
        /// object using the specified Action.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the field/method information.
        /// </typeparam>
        /// <param name="all">
        /// The collection of the field/method.
        /// </param>
        /// <param name="getAnnotation">
        /// The function mapping of a field/method to the attribute name.
        /// </param>
        /// <param name="getType">
        /// The function mapping of a field/method to the type.
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
            Func<T, XmlQualifiedName> getAnnotation,
            Func<T, Type> getType,
            Func<T, Injector> toInjector,
            Action<XmlQualifiedName, Reflector<string>> put)
            where T : MemberInfo
        {
            foreach (var m in all)
            {
                var name = getAnnotation(m);
                var type = getType(m);
                var sugarcoater = StringSugarcoaters.Of(type);
                var reflector = new Reflector<string>(
                    toInjector(m), type, sugarcoater);
                put(name, reflector);
            }
        }
    }
}
