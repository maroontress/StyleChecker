#pragma warning disable CS8019

namespace StyleChecker.Test.Refactoring.TypeClassParameter
{
    using System;

    public sealed class ReferencingCode
    {
        public void CallWithStringType()
        {
            ReferencedCode.PrintMethod(typeof(string));

            var code = new ReferencedCode();
            code.Print<object>(typeof(string));
        }

        public void CallWithIntType()
        {
            ReferencedCode.PrintMethod(typeof(int));

            var code = new ReferencedCode();
            code.Print<object>(typeof(int));
        }
    }
}
