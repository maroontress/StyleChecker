#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass
{
    public static class ReferencedCode_2
    {
        public static void Method<T>(T instance)
        {
        }

        public static T Identity<T>(T instance) => instance;
    }

    public sealed class ReferencedCode_1
    {
    }
}
