#pragma warning disable CS0219

namespace StyleChecker.Test.Refactoring.NotOneShotInitialization
{
    public sealed class Okay
    {
        public void Declaration()
        {
            var v = 0;
        }
    }
}
