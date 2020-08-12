#pragma warning disable CS0168
#pragma warning disable CS0219

namespace StyleChecker.Test.Refactoring.NotOneShotInitialization
{
    public sealed class Code
    {
        public void DeclareIfBlockAssign(bool b)
        {
            var v = 0;
            //@ ^v
            if (b)
            {
                v = 1;
            }
        }

        public void DeclareIfAssign(bool b)
        {
            var w = 0;
            //@ ^w
            if (b)
                w = 1;
        }

        public void DeclareTogetherIf(bool b)
        {
            int v = 0, w, x = 1, y = 2, z = 3;
            //@           ^x
            if (b)
            {
                x = 2;
            }
        }

        public void DeclareIfBlockAssign2(bool b)
        {
            var i = 0;
            //@ ^i
            var j = 1;
            //@ ^j
            if (b)
            {
                i = 1;
                j = 2;
            }
        }

        public void DeclareTogetherIf2(bool b)
        {
            int v = 0, w, x = 1, y = 2, z = 3;
            //@           ^x
            //@                  ^y
            if (b)
            {
                x = 2;
                y = 4;
            }
        }

        public void DeclareSwitchCaseAssignBreak(string s)
        {
            var v = 0;
            //@ ^v
            switch (s)
            {
                case "a":
                    v = 1;
                    break;
            }
        }

        public void DeclareSwitch3xCaseAssignBreak(string s)
        {
            var v = 0;
            //@ ^v
            switch (s)
            {
                case "a":
                    v = 1;
                    break;
                case "b":
                    v = 2;
                    break;
                case "c":
                    v = 3;
                    break;
            }
        }

        public void DeclareSwitchCaseAssignBreakDefaultBreak(string s)
        {
            var v = 0;
            //@ ^v
            switch (s)
            {
                case "a":
                    v = 1;
                    break;
                default:
                    break;
            }
        }

        public void DeclareSwitchCaseAssignBreak2(string s)
        {
            var v = 0;
            //@ ^v
            var w = 1;
            //@ ^w
            switch (s)
            {
                case "a":
                    v = 1;
                    w = 2;
                    break;
            }
        }

        public void DeclareSwitch3xCaseAssignBreak2(string s)
        {
            var v = 0;
            //@ ^v
            var w = 1;
            //@ ^w
            switch (s)
            {
                case "a":
                    v = 1;
                    w = 2;
                    break;
                case "b":
                    v = 2;
                    w = 3;
                    break;
                case "c":
                    v = 3;
                    w = 5;
                    break;
            }
        }
    }
}
