#nullable enable
#pragma warning disable CS8602

namespace StyleChecker.Test.Refactoring.IsNull
{
    public sealed class Code
    {
        public void EqualToNull(string? value)
        {
            if (!(value is
                {
                }))
            {
                return;
            }
        }

        public void NotEqualToNull(string? value)
        {
            if (value is
                {
                })
            {
                return;
            }
        }

        public void NullableValueType(int? value)
        {
            if (!(value is
                {
                }))
            {
                return;
            }
        }

        public void KeepTrivia(string? value)
        {
            if ( /*A*/  /*B*/  /*C*/ value /*D*/ is /*E*/
                {
                } /*F*/  /*G*/ )
            {
                return;
            }
        }

        public void EndOfLineTrivia(object? value)
        {
            if (value is
                {
                }
                && value.ToString() == "")
            {
                return;
            }
        }
    }
}
