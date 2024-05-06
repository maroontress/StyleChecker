#nullable enable

namespace StyleChecker.Test.Refactoring.EqualsNull
{
    public sealed class Code
    {
        public void EqualToNull(string? value)
        {
            if (value is null)
            {
                return;
            }
        }

        public void NotEqualToNull(string? value)
        {
            if (!(value is null))
            {
                return;
            }
        }

        public void NullableValueType(int? value)
        {
            if (value is null)
            {
                return;
            }
        }

        public void EndOfLineTrivia(object? value)
        {
            if (!(value is null)
                && value.ToString() == "")
            {
                return;
            }
        }
    }
}
