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
            var length = GetLength(new object[0]);
            //@                    ^object
        }

        public void WithInitializer()
        {
            var all = new string[] {};
            //@       ^string
        }

        public void WithZeroLength()
        {
            var all = new int[0];
            //@       ^int
        }

        public void Mixed()
        {
            var all = new object[0] {};
            //@       ^object
        }
    }
}
