#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass
{
    public static class Code<T>
        where T : class
    {
        public static void Method(T instance)
        {
        }
    }

    public static class NoConstraintClause<T>
    {
        public static void Method(T instance)
        {
        }
    }

    public static class MultipleTypeParameter<T>
        where T : class
    {
        public static void Method<U>(T instance, U option)
            where U : class
        {
        }
    }
}
