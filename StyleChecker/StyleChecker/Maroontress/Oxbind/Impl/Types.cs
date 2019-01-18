namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Provides methods to convert a type to another type, or to the object
    /// associated with the type.
    /// </summary>
    public static class Types
    {
        /// <summary>
        /// Gets the type of <see cref="IEnumerable{T}"/>.
        /// </summary>
        public static Type IEnumerableT { get; } = typeof(IEnumerable<>);

        /// <summary>
        /// Gets the type of <see cref="BindEvent{T}"/>.
        /// </summary>
        public static Type BindEventT { get; } = typeof(BindEvent<>);

        /// <summary>
        /// Gets the type of <see cref="BindEventImpl{T}"/>.
        /// </summary>
        public static Type BindEventImplT { get; } = typeof(BindEventImpl<>);

        /// <summary>
        /// Gets the type of <see cref="string"/>.
        /// </summary>
        public static Type String { get; } = typeof(string);

        /// <summary>
        /// Gets the type of <see cref="void"/>.
        /// </summary>
        public static Type Void { get; } = typeof(void);

        /// <summary>
        /// Gets the type of <see cref="BindEvent{T}"/> (<c>T</c> is
        /// <c>string</c>).
        /// </summary>
        public static Type BindEventString { get; }
            = typeof(BindEvent<string>);

        /// <summary>
        /// Returns the placeholder type of the specified type.
        /// </summary>
        /// <param name="t">
        /// The type.
        /// </param>
        /// <returns>
        /// The placeholder type.
        /// </returns>
        public static Type PlaceholderType(Type t)
        {
            if (!IsRawType(t, IEnumerableT))
            {
                return IsRawType(t, BindEventT)
                    ? FirstInnerType(t)
                    : t;
            }
            var a = FirstInnerType(t);
            return IsRawType(a, BindEventT)
                ? IEnumerableT.MakeGenericType(FirstInnerType(a))
                : t;
        }

        /// <summary>
        /// Gets the type of the first type parameter of the specified
        /// generic type.
        /// </summary>
        /// <param name="t">
        /// The generic type.
        /// </param>
        /// <returns>
        /// The type of the first type parameter.
        /// </returns>
        public static Type FirstInnerType(Type t)
            => t.GenericTypeArguments[0];

        /// <summary>
        /// Gets whether the raw type of the specified type is equal to the
        /// specified expected type.
        /// </summary>
        /// <param name="type">
        /// The generic type.
        /// </param>
        /// <param name="expectedType">
        /// The expected type, which is the generic type definition of the
        /// <paramref name="type"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if <paramref name="type"/> is generic type
        /// and if its generic type definition equals to
        /// <paramref name="expectedType"/>, or if <paramref name="type"/>
        /// is not generic type and it equals equals to
        /// <paramref name="expectedType"/>. <c>false</c> otherwise.
        /// </returns>
        public static bool IsRawType(Type type, Type expectedType)
            => ToRawType(type).Equals(expectedType);

        private static Type ToRawType(Type t)
        {
            var info = t.GetTypeInfo();
            return info.IsGenericType
                ? info.GetGenericTypeDefinition() : t;
        }
    }
}
