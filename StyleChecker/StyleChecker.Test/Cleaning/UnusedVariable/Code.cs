namespace StyleChecker.Test.Cleaning.UnusedVariable
{
    using System.Collections.Generic;

    public sealed class Code
    {
        public Code(int unused)
        {
        }

        public void Local()
        {
            var s = "" + 0;
        }

        public void Parameter(int unused)
        {
        }

        public void PatternMatching(object o)
        {
            if (o is string s)
            {
            }
        }

        public void OutVar(Dictionary<string, string> map)
        {
            if (map.TryGetValue("key", out var v))
            {
            }
        }
    }
}
