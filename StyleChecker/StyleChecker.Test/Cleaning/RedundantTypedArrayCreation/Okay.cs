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

        public void SpecifiedLength()
        {
            var all = new string[1] { "a", };
        }

        public void TwoDimensionSpecifiedLength()
        {
            var all = new string[2, 2] { { "a", "b", }, { "c", "d", }, };
        }

        public void FixedLengthArray()
        {
            var all = new string[3];
        }
    }
}
