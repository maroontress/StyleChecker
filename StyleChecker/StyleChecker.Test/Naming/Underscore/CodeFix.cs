namespace Application
{
    using System;

    public sealed class Code
    {
        public void LocalVarDecl()
        {
            var alpha = 1;
            int fooBar = 2;
            int fooBarBaz = 3;
        }

        public void Param(string[] args)
        {
            void outString(out string s)
            {
                s = "hello world";
            };
            outString(out string hello);
            Action<int> action
                = n => Console.WriteLine($"{n}");
            Action<int, int> action2
                = (n, m) => Console.WriteLine($"{n},{m}");
            void localFunc(int v)
            {
                Console.WriteLine($"{v}");
            }
        }
    }
}
