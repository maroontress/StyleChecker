namespace StyleChecker.Test.Refactoring.StinkyBooleanExpression
{
    using System;

    public sealed class Code
    {
        public void UseAnd(bool b1, bool b2)
        {
            _ = ((b1) && (b2));
            _ = ((b1) && (b2));
            _ = (!(b1) && (b2));

            _ = ((b1) && (b2));
            _ = ((b1) && (b2));
            _ = (!(b1) && (b2));

            _ = ((b1) && (b2));
            _ = ((b1) && (b2));
            _ = (!(b1) && (b2));
        }

        public void UseAnd(Func<bool> b1, Func<bool> b2, Func<bool> b3)
        {
            _ = ((b1() || b2()) && (b3()));
        }

        public void UseOr(bool b1, bool b2)
        {
            _ = ((b1) || (b2));
            _ = ((b1) || (b2));
            _ = (!(b1) || (b2));
        }
    }
}
