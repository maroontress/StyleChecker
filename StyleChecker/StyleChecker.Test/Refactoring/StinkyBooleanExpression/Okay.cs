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
    }
}
