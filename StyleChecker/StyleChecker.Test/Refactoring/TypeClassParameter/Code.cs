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

        public void PrintMethod(Type type)
        //@                          ^type,M,PrintMethod
        {
            Log(type.FullName);
        }

        public void CallWithStringType()
        {
            PrintMethod(typeof(string));
        }

        public void CallWithIntType()
        {
            PrintMethod(typeof(int));
        }
    }
}
