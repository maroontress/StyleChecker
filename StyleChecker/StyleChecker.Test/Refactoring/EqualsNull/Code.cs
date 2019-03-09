namespace StyleChecker.Test.Refactoring.EqualToNull
{
    public sealed class Code
    {
        public void EqualToNull(string value)
        {
            if (value == null)
            //@ ^==
            {
                return;
            }
        }

        public void NotEqualToNull(string value)
        {
            if (value != null)
            //@ ^!=
            {
                return;
            }
        }

        public void NullableValueType(int? value)
        {
            if (value == null)
            //@ ^==
            {
                return;
            }
        }
    }
}
