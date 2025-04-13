namespace StyleChecker.Test.Refactoring.AssignmentToParameter
{
    public sealed class Okay
    {
        private string message;

        public string Property {
            get
            {
                return message;
            }

            set
            {
                message = value;
            }
        }

        public void Ref(ref int value, out object o)
        {
            value = 0;
            o = null;
        }
    }
}
