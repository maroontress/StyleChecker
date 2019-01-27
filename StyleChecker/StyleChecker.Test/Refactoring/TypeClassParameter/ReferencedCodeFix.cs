#pragma warning disable CS8019

namespace StyleChecker.Test.Refactoring.TypeClassParameter
{
    using System;

    public sealed class ReferencedCode
    {
        public static void Log(string message)
        {
        }

        public static void PrintMethod<T>()
        {
            var type = typeof(T);
            Log(type.FullName);
        }

        public void Print<T, T0>()
        {
            var type = typeof(T0);
            Log(type.FullName);
        }
    }
}
