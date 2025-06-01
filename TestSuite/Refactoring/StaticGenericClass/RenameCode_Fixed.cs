#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass;

public static class RenameCode_1
{
    public static void Method<T>(T instance) where T : class
    {
    }
}

public static class RenameCode
{
}

public static class System_1
{
    public static void Method<T>(T instance) where T : class
    {
    }
}

public static class String_1
{
    public static void Method<T>(T instance) where T : class
    {
    }
}
