#pragma warning disable CS8321

namespace StyleChecker.Test.Refactoring.TypeClassParameter;

using System;

public sealed class LocalFunctionOkay
{
    public void IncludesStaticClass()
    {
        void Print(Type t)
        {
            Console.WriteLine(t.FullName);
        }

        Print(typeof(string));
        Print(typeof(StaticClass));
    }

    public void NotAllArgumentsAreTypeof()
    {
        void Print(Type t)
        {
            Console.WriteLine(t.FullName);
        }

        Print(typeof(string));
        Print(GetType());
    }

    public void NobodyInvokes()
    {
        void NeverUsed(Type t)
            => Console.WriteLine(t.FullName);
    }

    public void MethodRederence()
    {
        string ToName(Type t) => t.FullName;

        void Print(Func<Type, string> toName, Type[] array)
        {
            foreach (var i in array)
            {
                Console.Write(toName(i));
            }
        }

        var name = ToName(typeof(object));
        Print(ToName, [typeof(string), typeof(int)]);
    }

    public static class StaticClass;
}
