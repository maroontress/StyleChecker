#pragma warning disable CS0168

namespace StyleChecker.Test.Cleaning.UnusedVariable
{
    using System;
    using System.Collections.Generic;
    using StyleChecker.Annotations;

    public sealed class Code
    {
        public Code(int unused)
            //@         ^${p},unused,${neverUsed}
        {
        }

        public void Local()
        {
            var s = "" + 0;
            //@ ^${v},s,${neverUsed}
        }

        public void Parameter(int unused)
            //@                   ^${p},unused,${neverUsed}
        {
        }

        public void PatternMatching(object o)
        {
            if (o is string s)
            //@             ^${v},s,${neverUsed}
            {
            }
        }

        public void Catch()
        {
            try
            {
            }
            catch (Exception e)
            //@              ^${v},e,${neverUsed}
            {
            }
        }

        public void ForEach()
        {
            var all = new[] { "a", "b", "c", };
            foreach (var e in all)
            //@          ^${v},e,${neverUsed}
            {
            }
        }

        public void OutVar(Dictionary<string, string> map)
        {
            if (map.TryGetValue("key", out var v))
            //@                                ^${v},v,${neverUsed}
            {
            }
        }

        public void UsedButMarkedAsUnused([Unused] int ignored)
            //@                                        ^${p},ignored,${usedButMarked}
        {
            Parameter(ignored);
        }

        public void StartWithAt(int @baz, Dictionary<string, string> map)
            //@                     ^${p},@baz,${neverUsed}
        {
            var @foo = "string";
            //@ ^${v},@foo,${neverUsed}
            if ("foo" is string @stringFoo)
            //@                 ^${v},@stringFoo,${neverUsed}
            {
            }
            if (map.TryGetValue("key", out var @bar))
            //@                                ^${v},@bar,${neverUsed}
            {
            }
        }
    }

    public abstract class BaseClass
    {
        public abstract void Method([Unused] int unused);
            //@                                  ^${p},unused,${unnecessaryMark}
        public virtual void CustomizePoint([Unused] int unused)
            //@                                         ^${p},unused,${unnecessaryMark}
        {
        }
    }
}
