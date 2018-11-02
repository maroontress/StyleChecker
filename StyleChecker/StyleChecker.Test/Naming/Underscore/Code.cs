#pragma warning disable CS0219
#pragma warning disable CS8321

namespace Application
{
    using System;

    public sealed class Code
    {
        public void LocalVarDecl()
        {
            var _alpha = 1;
            int foo_bar = 2;
            int _foo_bar_baz_ = 3;
        }

        public void Param(string[] _args)
        {
            void outString(out string _s)
            {
                _s = "hello world";
            };
            outString(out string _hello);
            Action<int> _action
                = _n => Console.WriteLine($"{_n}");
            Action<int, int> _action2
                = (_n, _m) => Console.WriteLine($"{_n},{_m}");
            void _localFunc(int _v)
            {
                Console.WriteLine($"{_v}");
            }
            if (this is object _o)
            {
                Console.WriteLine($"{_o}");
            }
        }

        public void Catch()
        {
            try
            {
            }
            catch (Exception _e)
            {
                Console.WriteLine($"{_e}");
            }
        }

        public void ForEach()
        {
            foreach (var _item in new[] { "a", "b", "c" })
            {
            }
        }
    }
}
