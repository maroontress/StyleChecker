namespace StyleChecker.Test.Refactoring.StinkyBooleanExpression
{
    using System;

    public sealed class Code
    {
        public void UseAnd(bool b1, bool b2)
        {
            _ = !b1 ? false : b2;
            //@ ^This conditional operator can be replaced with '&&' operator.
            _ = b1 ? b2 : false;
            //@ ^This conditional operator can be replaced with '&&' operator.
            _ = b1 ? false : b2;
            //@ ^This conditional operator can be replaced with '&&' operator.

            _ = (!b1 ? false : b2);
            //@  ^This conditional operator can be replaced with '&&' operator.
            _ = (b1 ? b2 : false);
            //@  ^This conditional operator can be replaced with '&&' operator.
            _ = (b1 ? false : b2);
            //@  ^This conditional operator can be replaced with '&&' operator.

            _ = !(b1) ? false : (b2);
            //@ ^This conditional operator can be replaced with '&&' operator.
            _ = (b1) ? (b2) : false;
            //@ ^This conditional operator can be replaced with '&&' operator.
            _ = (b1) ? false : (b2);
            //@ ^This conditional operator can be replaced with '&&' operator.
        }

        public void UseAnd(Func<bool> b1, Func<bool> b2, Func<bool> b3)
        {
            _ = !(b1() || b2()) ? false : b3();
            //@ ^This conditional operator can be replaced with '&&' operator.
        }

        public void UseOr(bool b1, bool b2)
        {
            _ = !b1 ? b2 : true;
            //@ ^This conditional operator can be replaced with '||' operator.
            _ = b1 ? true : b2;
            //@ ^This conditional operator can be replaced with '||' operator.
            _ = b1 ? b2 : true;
            //@ ^This conditional operator can be replaced with '||' operator.
        }
    }
}
