#pragma warning disable CS0168
#pragma warning disable CS0219

namespace StyleChecker.Test.Refactoring.UninitializedLocalVariable
{
    public sealed class Code
    {
        public static void Main()
        {
            int foo;
            //@ ^foo
            int bar = 0, baz;
            //@          ^baz
        }
    }
}
