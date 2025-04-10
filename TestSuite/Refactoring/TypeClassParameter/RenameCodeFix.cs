#pragma warning disable CS8019

namespace StyleChecker.Test.Refactoring.TypeClassParameter
{
    using System;

    public sealed class RenameCode
    {
        private void PrintMethod_0<T>()
        {
        }

        private void PrintMethod<T>()
        {
            var type = typeof(T);
        }

        public void CallWithStringType()
        {
            PrintMethod<string>();
            PrintMethod_0<int>();
        }
    }
}
