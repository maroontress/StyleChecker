#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass;

public static class Okay0<T0>
{
}

public static class Okay1<T1>
{
    public static void Method<T1>(T1 instance)
    {
    }
}

public static class Okay2<T2>
{
    public static void Method<T1>(T1 instance)
    {
    }
}
