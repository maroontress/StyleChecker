namespace StyleChecker.Test.Cleaning.RedundantTypedArrayCreation
{
    public sealed class Okay
    {
        public void Code(int count)
        {
            var all = new string[count];
        }
    }
}
