#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass
{
    using System.Linq;

    public static class ReferencingCode
    {
        public static void Method()
        {
            ReferencedCode.Method("string");
            ReferencedCode.Method(typeof(string));
            var type = typeof(ReferencedCode);
            System.Action<string> m = ReferencedCode.Method;

            string[] inArray = { "a", "b", "c" };
            var outArray = inArray.Select(ReferencedCode.Identity)
                .ToArray();
        }
    }
}
