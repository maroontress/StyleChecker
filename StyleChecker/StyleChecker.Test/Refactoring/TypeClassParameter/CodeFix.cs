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

        public void NGExpression()
        {
            void PrintExpression<T>()
            {
                var type = typeof(T);
                Log(type.FullName);
            }

            PrintExpression<string>();
            PrintExpression<int>();
        }

        public void NGGenerics()
        {
            void PrintGenerics<T, T0>()
            {
                var type = typeof(T0);
                Log(type.FullName);
            }

            PrintGenerics<object, string>();
            PrintGenerics<object, int>();
        }

        public void PrintMethod<T>()
        {
            var type = typeof(T);
            Log(type.FullName);
        }

        public void PrintMethodExpression<T>()
        {
            var type = typeof(T);
            Log(type.FullName);
        }

        public void PrintGenericsMethod<T, T0>()
        {
            var type = typeof(T0);
            Log(type.FullName);
        }

        public void CallWithStringType()
        {
            PrintMethod<string>();
            PrintMethodExpression<string>();
            PrintGenericsMethod<object, string>();
        }

        public void CallWithIntType()
        {
            PrintMethod<int>();
            PrintMethodExpression<int>();
            PrintGenericsMethod<object, int>();
        }
    }
}
