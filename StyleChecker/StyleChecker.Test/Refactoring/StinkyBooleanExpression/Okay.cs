using System;

namespace StyleChecker.Test.Refactoring.StinkyBooleanExpression
{
    public sealed class Okay
    {
        public void BothBoolLiteral(bool b)
        {
            _ = b ? true : false;
        }

        public void BothNonBoolLiteral(bool b, bool whenTrue, bool whenFalse)
        {
            _ = b ? whenFalse : whenTrue;
        }

        public void ThrowStatement(bool b1, bool b2)
        {
            _ = b1 ? throw new Exception() : false;
            _ = b2 ? true : throw new Exception();
        }
    }
}
