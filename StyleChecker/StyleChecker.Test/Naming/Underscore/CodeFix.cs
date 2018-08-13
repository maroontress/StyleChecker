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
            var alpha = 1;
            int fooBar = 2;
            int fooBarBaz = 3;
            void outString(out string s);
            {
                s = "hello world";
            };
            outString(out string hello);
        }
    }
}
