namespace StyleChecker.Test.Refactoring.DiscardingReturnValue
{
    public sealed class Methods
    {
        public void IntParse()
        {
            int.Parse("32");
        //  ^System.Int32.Parse(string)
        }

        public void BooleanParse()
        {
            bool.Parse("True");
        //  ^System.Boolean.Parse(string)
        }
    }
}
