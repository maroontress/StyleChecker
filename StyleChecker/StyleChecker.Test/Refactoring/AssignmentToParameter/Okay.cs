namespace StyleChecker.Test.Refactoring.AssignmentToParameter
{
    public sealed class Okay
    {
        public void Ref(ref int value, out object o)
        {
            value = 0;
            o = null;
        }
    }
}
