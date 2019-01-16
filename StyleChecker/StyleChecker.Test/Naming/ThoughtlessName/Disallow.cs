#pragma warning disable CS0219

namespace StyleChecker.Test.Naming.ThoughtlessName
{
    public sealed class Disallow
    {
        public void Identifiers()
        {
            var flag = true;
            //@ ^flag
            var flags = 0x1 | 0x2;
            //@ ^flags
        }
    }
}
