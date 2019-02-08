namespace StyleChecker.Test.Cleaning.RedundantTypedArrayCreation
{
    public sealed class Okay
    {
        public void Complex()
        {
            var all = new object[] { "a", 1, };
        }

        public void Empty()
        {
            var all = new string[] {};
        }

        public void Null()
        {
            var all = new string[] { null, };
        }

        public void Implicit()
        {
            var all = new[] { "a", };
        }
    }
}
