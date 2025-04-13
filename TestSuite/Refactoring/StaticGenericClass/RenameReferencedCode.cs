#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass
{
    public static class ReferencedCode<T>
    {
        public static void Method(T instance)
        {
        }

        public static T Identity(T instance) => instance;
    }

    public sealed class ReferencedCode_1
    {
    }
}
