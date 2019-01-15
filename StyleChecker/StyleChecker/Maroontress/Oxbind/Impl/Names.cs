namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    ///  Creates various joined names of the class, method, field.
    /// </summary>
    public static class Names
    {
        /// <summary>
        /// The initial capacity of the <see cref="StringBuilder"/>.
        /// </summary>
        private const int InitialCapacity = 80;

        /// <summary>
        /// The common delimiter between names.
        /// </summary>
        private const string Delimiter = ", ";

        /// <summary>
        /// Returns the joined string of the specified strings with the default
        /// delimiter.
        /// </summary>
        /// <param name="all">
        /// The <c>string</c>s.
        /// </param>
        /// <returns>
        /// The joined string.
        /// </returns>
        public static string Join(IEnumerable<string> all)
        {
            return string.Join(Delimiter, all);
        }

        /// <summary>
        /// Sorts the specified strings and returns the joined string of them
        /// with the default delimiter.
        /// </summary>
        /// <param name="all">
        /// The <c>string</c>s.
        /// </param>
        /// <returns>
        /// The sorted and joined string.
        /// </returns>
        public static string SortAndJoin(IEnumerable<string> all)
        {
            return Join(all.OrderBy(s => s));
        }

        /// <summary>
        /// Returns the name of the specified method with the default format.
        /// </summary>
        /// <param name="method">
        /// A method.
        /// </param>
        /// <returns>
        /// The name of the method with the default format.
        /// </returns>
        public static string GetMethodName(MethodInfo method)
        {
            var b = new StringBuilder(InitialCapacity);
            return b.Append(method.Name)
                .Append("(")
                .Append(Join(method.GetParameters()
                    .Select(p => p.ParameterType.Name)))
                .Append(")").ToString();
        }

        /// <summary>
        /// Returns the joined string of the class names with the default
        /// delimiter.
        /// </summary>
        /// <param name="all">
        /// The classes.
        /// </param>
        /// <returns>
        /// The joined string.
        /// </returns>
        public static string OfClasses(IEnumerable<Type> all)
        {
            return MemberNames(all, Of);
        }

        /// <summary>
        /// Returns the joined string of the method names with the default
        /// delimiter.
        /// </summary>
        /// <param name="all">
        /// The methods.
        /// </param>
        /// <returns>
        /// The joined string.
        /// </returns>
        public static string OfMethods(IEnumerable<MethodInfo> all)
        {
            return MemberNames(all, GetMethodName);
        }

        /// <summary>
        /// Returns the joined string of the field names with the default
        /// delimiter.
        /// </summary>
        /// <param name="all">
        /// The fields.
        /// </param>
        /// <returns>
        /// The joined string.
        /// </returns>
        public static string OfFields(IEnumerable<FieldInfo> all)
        {
            return MemberNames(all, m => m.Name);
        }

        /// <summary>
        /// Gets the name representing the specified type.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The name of the type.
        /// </returns>
        public static string Of(Type type)
        {
            if (!type.GetTypeInfo().IsGenericType)
            {
                return type.Name;
            }
            var arguments = type.GenericTypeArguments;
            var argumentNames = arguments.Select(Of);
            var name = type.Name;
            var index = name.IndexOf('`');
            name = name.Substring(0, index);
            var genericName = $"{name}<{string.Join(",", argumentNames)}>";
            return genericName;
        }

        /// <summary>
        /// Sorts the names of the specified objects and returns the joined
        /// string of them with the default format.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the objects.
        /// </typeparam>
        /// <param name="all">
        /// The objects.
        /// </param>
        /// <param name="getName">
        /// The function that returns the name of its argument.
        /// </param>
        /// <returns>
        /// The joined string.
        /// </returns>
        private static string MemberNames<T>(
            IEnumerable<T> all, Func<T, string> getName)
        {
            return Join(all.Select(m => getName(m))
                .OrderBy(s => s));
        }
    }
}
