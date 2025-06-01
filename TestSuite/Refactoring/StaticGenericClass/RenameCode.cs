#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass;

public static class RenameCode<T>
    where T : class
{
    public static void Method(T instance)
    {
    }
}

public static class RenameCode
{
}

public static class System<T>
    where T : class
{
    public static void Method(T instance)
    {
    }
}

public static class String<T>
    where T : class
{
    public static void Method(T instance)
    {
    }
}
