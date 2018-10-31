namespace StyleChecker.Test.Refactoring.AssignmentToParameter
{
    public sealed class Code
    {
        public void NG(int value, object o)
        {
            value = 0;
            o = null;
        }
    }
}
