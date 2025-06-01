#pragma warning disable CS8019

namespace StyleChecker.Test.Refactoring.TypeClassParameter;

using System;

public sealed class ReferencingCode
{
    public void CallWithStringType()
    {
        ReferencedCode.PrintMethod<string>();

        var code = new ReferencedCode();
        code.Print<object, string>();
    }

    public void CallWithIntType()
    {
        ReferencedCode.PrintMethod<int>();

        var code = new ReferencedCode();
        code.Print<object, int>();
    }
}
