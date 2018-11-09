namespace StyleChecker.Test.Refactoring.AssignmentToParameter
{
    public sealed class Code
    {
        public void NG(int value, object o)
        {
            value = 0;
            o = null;
        }

        public void Add(int value)
        {
            value += 1;
        }

        public void Increment(int value)
        {
            ++value;
            value++;
        }

        public void Decrement(int value)
        {
            --value;
            value--;
        }

        public void PassByReference(int value)
        {
            Clear(ref value);
        }

        public void Clear(ref int value)
        {
            value = 0;
        }

        public void PassOutVar(int value)
        {
            GetValue(out value);
        }

        public void GetValue(out int value)
        {
            value = 0;
        }
    }
}
