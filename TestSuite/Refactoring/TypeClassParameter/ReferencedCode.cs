#pragma warning disable CS8019

namespace StyleChecker.Test.Refactoring.TypeClassParameter
{
    using System;

    public sealed class ReferencedCode
    {
        private static void Log(string message)
        {
        }

        /// <summary>
        /// Print the specified type.
        /// </summary>
        /// <param name="type">
        /// The type to be printed.
        /// </param>
        public static void PrintMethod(Type type)
        {
            Log(type.FullName);
        }

        /// <summary>
        /// Print the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type to be ignored.
        /// </typeparam>
        /// <param name="type">
        /// The type to be printed.
        /// </param>
        public void Print<T>(Type type)
        {
            Log(type.FullName);
        }
    }
}
