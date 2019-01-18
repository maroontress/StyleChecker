namespace Maroontress.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// The utility class for <c>class</c> objects.
    /// </summary>
    public static class Classes
    {
        /// <summary>
        /// Returns the list of instance fields of the specified class
        /// annotated with the specified annotation.
        /// </summary>
        /// <param name="clazz">
        /// The class.
        /// </param>
        /// <typeparam name="T">
        /// The annotation to the instance fields.
        /// </typeparam>
        /// <returns>
        /// The list of instance fields.
        /// </returns>
        public static IEnumerable<FieldInfo> GetInstanceFields<T>(Type clazz)
            where T : Attribute
        {
            return clazz.GetTypeInfo()
                .DeclaredFields
                .Where(f => !f.IsStatic
                    && f.GetCustomAttribute<T>() != null);
        }

        /// <summary>
        /// Returns the list of instance methods of the specified class
        /// annotated with the specified annotation.
        /// </summary>
        /// <param name="clazz">
        /// The class.
        /// </param>
        /// <typeparam name="T">
        /// The annotation to the instance methods.
        /// </typeparam>
        /// <returns>
        /// The list of instance methods.
        /// </returns>
        public static IEnumerable<MethodInfo> GetInstanceMethods<T>(Type clazz)
            where T : Attribute
        {
            return clazz.GetTypeInfo()
                .DeclaredMethods
                .Where(m => !m.IsStatic
                    && m.GetCustomAttribute<T>() != null);
        }

        /// <summary>
        /// Returns the list of static fields of the specified class annotated
        /// with the specified annotation.
        /// </summary>
        /// <param name="clazz">
        /// The class.
        /// </param>
        /// <typeparam name="T">
        /// The annotation to the static fields.
        /// </typeparam>
        /// <returns>
        /// The list of static fields.
        /// </returns>
        public static IEnumerable<FieldInfo> GetStaticFields<T>(Type clazz)
            where T : Attribute
        {
            return clazz.GetTypeInfo()
                .DeclaredFields
                .Where(f => f.IsStatic
                    && f.GetCustomAttribute<T>() != null);
        }

        /// <summary>
        /// Returns the list of static methods of the specified class annotated
        /// with the specified annotation.
        /// </summary>
        /// <param name="clazz">
        /// The class.
        /// </param>
        /// <typeparam name="T">
        /// The annotation to the static methods.
        /// </typeparam>
        /// <returns>
        /// The list of static methods.
        /// </returns>
        public static IEnumerable<MethodInfo> GetStaticMethods<T>(Type clazz)
            where T : Attribute
        {
            return clazz.GetTypeInfo()
                .DeclaredMethods
                .Where(m => m.IsStatic
                    && m.GetCustomAttribute<T>() != null);
        }
    }
}
