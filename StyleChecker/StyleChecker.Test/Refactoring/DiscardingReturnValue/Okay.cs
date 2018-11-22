namespace StyleChecker.Test.Refactoring.DiscardingReturnValue
{
    using System;
    using System.Reflection;

    public sealed class Okay
    {
        public void StringClass()
        {
            var s = "Hello, World.";
            var array = new char[s.Length];
            s.CopyTo(0, array, 0, s.Length);
        }

        public void TypeClass()
        {
            var s = "Hello, World.";
            var t = s.GetType();
            var b = Type.DefaultBinder;
            t.InvokeMember(
                "ToString",
                BindingFlags.InvokeMethod,
                b,
                t,
                new object[] {});
        }
    }
}
