#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass
{
    using System.Linq;

    public static class ReferencingCode
    {
        public static void Method()
        {
            ReferencedCode_2.Method("string");
            ReferencedCode_2.Method(typeof(string));
            var type = typeof(ReferencedCode_2);
            System.Action<string> m = ReferencedCode_2.Method;

            string[] inArray = { "a", "b", "c" };
            var outArray = inArray.Select(ReferencedCode_2.Identity)
                .ToArray();
        }
    }

    public sealed class ReferencedCode
    {
    }
}
