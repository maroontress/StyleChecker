#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass
{
    public static class Code
    {
        public static void Method<T>(T instance) where T : class
        {
        }
    }

    public static class NoConstraintClause
    {
        public static void Method<T>(T instance)
        {
        }
    }

    public static class MultipleTypeParameter
    {
        public static void Method<T, U>(T instance, U option) where T : class
            where U : class
        {
        }
    }
}
