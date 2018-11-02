namespace StyleChecker.Test.Cleaning.UnusedVariable
{
    using System.Collections.Generic;
    using StyleChecker.Annotations;

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

        public void UsedButMarkedAsUnused([Unused] int ignored)
        {
            Parameter(ignored);
        }
    }

    public abstract class BaseClass
    {
        public abstract void Method([Unused] int unused);
        public virtual void CustomizePoint([Unused] int unused)
        {
        }
    }
}
