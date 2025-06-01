#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass;

public static class ReferencedCode
{
    public static void Method<T>(T instance)
    {
    }

    public static T Identity<T>(T instance) => instance;
}
