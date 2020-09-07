namespace StyleChecker.Test.Refactoring.IsNull
{
    public sealed class Code
    {
        public void EqualToNull(string value)
        {
            if (value == null)
            {
                return;
            }
        }

        public void NotEqualToNull(string value)
        {
            if (value != null)
            {
                return;
            }
        }

        public void NullableValueType(int? value)
        {
            if (value == null)
            {
                return;
            }
        }

        public void KeepTrivia(string value)
        {
            if ( /*A*/  /*B*/  /*C*/ value /*D*/ != /*E*/ null /*F*/  /*G*/ )
            {
                return;
            }
        }

        public void EndOfLineTrivia(object value)
        {
            if (value != null
                && value.ToString() == "")
            {
                return;
            }
        }
    }
}
