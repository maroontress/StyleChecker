#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass;

using System.Linq;

public static class ReferencingCode
{
    public static void Method()
    {
        ReferencedCode<string>.Method("string");
        ReferencedCode<System.Type>.Method(typeof(string));
        var type = typeof(ReferencedCode<object>);
        System.Action<string> m = ReferencedCode<string>.Method;

        string[] inArray = { "a", "b", "c" };
        var outArray = inArray.Select(ReferencedCode<string>.Identity)
            .ToArray();
    }
}
