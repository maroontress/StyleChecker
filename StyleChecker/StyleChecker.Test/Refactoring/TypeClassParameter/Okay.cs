#pragma warning disable CS8321

namespace StyleChecker.Test.Refactoring.TypeClassParameter
{
    using System;

    public sealed class Okay
    {
        public void NotAllArgumentsAreTypeofLocalFunction()
        {
            void Print(Type t)
            {
                Console.WriteLine(t.FullName);
            }

            Print(typeof(string));
            Print(GetType());
        }

        public void NobodyInvokesLocalFunction()
        {
            void NeverUsed(Type t)
                => Console.WriteLine(t.FullName);
        }

        public void NotAllArgumentsAreTypeof(Type type)
        {
            Console.WriteLine(type.FullName);
        }

        public void NobodyInvokes(Type type)
        {
            Console.WriteLine(type.FullName);
        }

        public void Invoke()
        {
            NotAllArgumentsAreTypeof(typeof(string));
            NotAllArgumentsAreTypeof(GetType());
        }
    }
}
