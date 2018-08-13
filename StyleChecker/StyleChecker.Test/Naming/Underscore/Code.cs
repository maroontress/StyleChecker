namespace Application
{
    using System;

    public sealed class Code
    {
        public void OK()
        {
        }

        public void NG()
        {
            var _alpha = 1;
            int foo_bar = 2;
            int _foo_bar_baz_ = 3;
            void outString(out string s);
            {
                s = "hello world";
            };
            outString(out string _hello);
        }
    }
}
