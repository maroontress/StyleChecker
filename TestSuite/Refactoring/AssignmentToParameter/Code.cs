namespace StyleChecker.Test.Refactoring.AssignmentToParameter
{
    public sealed class Code
    {
        public void NG(int value, object o)
        {
            value = 0;
//@         ^value
            o = null;
//@         ^o
        }

        public void Add(int value)
        {
            value += 1;
//@         ^value
        }

        public void Increment(int value)
        {
            ++value;
//@           ^value
            value++;
//@         ^value
        }

        public void Decrement(int value)
        {
            --value;
//@           ^value
            value--;
//@         ^value
        }

        public void PassByReference(int value)
        {
            Clear(ref value);
//@                   ^value
        }

        public void Clear(ref int value)
        {
            value = 0;
        }

        public void PassOutVar(int value)
        {
            GetValue(out value);
//@                      ^value
        }

        public void GetValue(out int value)
        {
            value = 0;
        }

        private string message = "value";

        public string Property {
            get
            {
                return message;
            }

            set
            {
                value = message;
//@             ^value
            }
        }
    }
}
