#pragma warning disable CS8019

namespace StyleChecker.Test.Refactoring.TypeClassParameter
{
    using System;

    public sealed class ReferencedCode
    {
        public static void Log(string message)
        {
        }

        public static void PrintMethod(Type type)
        {
            Log(type.FullName);
        }

        public void Print<T>(Type type)
        {
            Log(type.FullName);
        }
    }
}
