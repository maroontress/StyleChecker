#pragma warning disable CS8019

namespace StyleChecker.Test.Refactoring.TypeClassParameter
{
    using System;

    public sealed class Code
    {
        public static void Log(string message)
        {
        }

        public void NG()
        {
            void Print<T>()
            {
                var type = typeof(T);
                Log(type.FullName);
            }

            Print<string>();
            Print<int>();
        }

        public void PrintMethod<T>()
        {
            var type = typeof(T);
            Log(type.FullName);
        }

        public void CallWithStringType()
        {
            PrintMethod<string>();
        }

        public void CallWithIntType()
        {
            PrintMethod<int>();
        }
    }
}
