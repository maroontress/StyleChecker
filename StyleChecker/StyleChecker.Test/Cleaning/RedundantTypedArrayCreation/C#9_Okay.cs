namespace StyleChecker.Test.Cleaning.RedundantTypedArrayCreation
{
    using System;

    public sealed class Okay
    {
        public void MethodReferences()
        {
            var all = new Action[]
            {
                Complex,
                Empty,
            };
        }

        public void MethodReferencesWithNull()
        {
            var all = new Action[]
            {
                Complex,
                null,
            };
        }

        public void Empty()
        {
        }

        public void Complex()
        {
        }
    }
}
