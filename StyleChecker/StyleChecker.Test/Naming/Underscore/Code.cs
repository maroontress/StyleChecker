#pragma warning disable CS0219
#pragma warning disable CS8321

namespace Application
{
    using System;

    public sealed class Code
    {
        public void LocalVarDecl()
        {
            var _ = 0;
            //@ ^_
            var _alpha = 1;
            //@ ^_alpha
            int foo_bar = 2;
            //@ ^foo_bar
            int _foo_bar_baz_ = 3;
            //@ ^_foo_bar_baz_
        }

        public void Param(string[] _args)
            //@                    ^_args
        {
            void outString(out string _s)
                //@                   ^_s
            {
                _s = "hello world";
            }
            outString(out string _hello);
            //@                  ^_hello
            Action<int> _action
                //@     ^_action
                = _n => Console.WriteLine($"{_n}");
            //@   ^_n
            Action<int, int> _action2
                //@          ^_action2
                = (_n, _m) => Console.WriteLine($"{_n},{_m}");
            //@    ^_n
            //@        ^_m
            void _localFunc(int _v)
            //@  ^_localFunc
            //@                 ^_v
            {
                Console.WriteLine($"{_v}");
            }
            if (this is object _o)
                //@            ^_o
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
            //@              ^_e
            {
                Console.WriteLine($"{_e}");
            }
        }

        public void ForEach()
        {
            foreach (var _item in new[] { "a", "b", "c" })
                //@      ^_item
            {
            }
        }
    }
}
