namespace StyleChecker.Test.Cleaning.RedundantTypedArrayCreation
{
    public sealed class Code
    {
        public void ArgumentSyntax()
        {
            int GetLength(object[] array)
            {
                return array.Length;
            }
            var length = GetLength(System.Array.Empty<object>());
        }

        public void WithInitializer()
        {
            var all = System.Array.Empty<string>();
        }

        public void WithZeroLength()
        {
            var all = System.Array.Empty<int>();
        }

        public void Mixed()
        {
            var all = System.Array.Empty<object>();
        }
    }
}
