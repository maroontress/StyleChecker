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
            void Print(Type type)
            //@             ^type,L,Print
            {
                Log(type.FullName);
            }

            Print(typeof(string));
            Print(typeof(int));
        }

        public void NGExpression()
        {
            void PrintExpression(Type type) => Log(type.FullName);
            //@                       ^type,L,PrintExpression

            PrintExpression(typeof(string));
            PrintExpression(typeof(int));
        }

        public void NGGenerics()
        {
            void PrintGenerics<T>(Type type)
            //@                        ^type,L,PrintGenerics
            {
                Log(type.FullName);
            }

            PrintGenerics<object>(typeof(string));
            PrintGenerics<object>(typeof(int));
        }

        public void PrintMethod(Type type)
        //@                          ^type,M,PrintMethod
        {
            Log(type.FullName);
        }

        public void PrintMethodExpression(Type type) => Log(type.FullName);
        //@                                    ^type,M,PrintMethodExpression

        public void PrintGenericsMethod<T>(Type type)
        //@                                     ^type,M,PrintGenericsMethod
        {
            Log(type.FullName);
        }

        public void CallWithStringType()
        {
            PrintMethod(typeof(string));
            PrintMethodExpression(typeof(string));
            PrintGenericsMethod<object>(typeof(string));
        }

        public void CallWithIntType()
        {
            PrintMethod(typeof(int));
            PrintMethodExpression(typeof(int));
            PrintGenericsMethod<object>(typeof(int));
        }

        public void CallWithTypeParameter<T>()
        {
            PrintMethod(typeof(T));
            PrintMethodExpression(typeof(T));
            PrintGenericsMethod<object>(typeof(T));
            PrintGenericsMethod<T>(typeof(T));
        }
    }
}
