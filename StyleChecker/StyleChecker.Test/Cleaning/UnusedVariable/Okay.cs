namespace StyleChecker.Test.Cleaning.UnusedVariable
{
    using System.Collections.Generic;

    public sealed class Okay
    {
        private int used;

        public Okay(int used)
        {
            this.used = used;
        }

        public int GetValue()
        {
            var s = used;
            return s;
        }

        public void SetValue(int used)
        {
            this.used = used;
        }

        public void PatternMatching(object o)
        {
            if (o is string s)
            {
                used = int.Parse(s);
            }
        }

        public void OutVar(Dictionary<string, string> map)
        {
            if (map.TryGetValue("key", out var v))
            {
                used = int.Parse(v);
            }
        }
    }
}
