namespace StyleChecker.Test.Refactoring.EqualsNull
{
    public sealed class Okay
    {
        public void NonNullableValueType(int value)
        {
#pragma warning disable CS0472
            if (value == null)
#pragma warning restore CS0472
            {
                return;
            }
        }
    }
}
