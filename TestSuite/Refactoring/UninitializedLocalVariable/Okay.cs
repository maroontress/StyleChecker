#pragma warning disable CS0219

namespace StyleChecker.Test.Refactoring.UninitializedLocalVariable
{
    public sealed class Okay
    {
        public static void Main()
        {
            int foo = 0;
            int bar = foo, baz = 1;
        }
    }
}
