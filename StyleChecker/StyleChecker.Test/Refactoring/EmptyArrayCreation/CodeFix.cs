namespace StyleChecker.Test.Cleaning.RedundantTypedArrayCreation
{
    public sealed class Code
    {
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
