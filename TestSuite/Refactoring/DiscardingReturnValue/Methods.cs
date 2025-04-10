namespace StyleChecker.Test.Refactoring.DiscardingReturnValue
{
    using System;
    using System.Collections.Generic;

    public sealed class Methods
    {
        public void IntParse()
        {
            int.Parse("32");
        //@ ^int.Parse(string)
        }

        public void BooleanParse()
        {
            bool.Parse("True");
        //@ ^bool.Parse(string)
        }

        public void Parse()
        {
            var list = new List<string>();
            list.Contains("");
        //@ ^System.Collections.Generic.List<T>.Contains(T)

            Array.Empty<string>();
        //@ ^System.Array.Empty<T>()
        }
    }
}
