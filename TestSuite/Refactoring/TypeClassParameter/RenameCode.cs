#pragma warning disable CS8019

namespace StyleChecker.Test.Refactoring.TypeClassParameter
{
    using System;

    public sealed class RenameCode
    {
        private void PrintMethod<T>()
        {
        }

        private void PrintMethod(Type type)
        //@                           ^type,M,PrintMethod
        {
        }

        public void CallWithStringType()
        {
            PrintMethod(typeof(string));
            PrintMethod<int>();
        }
    }
}
