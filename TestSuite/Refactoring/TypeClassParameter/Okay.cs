#pragma warning disable CS8321

namespace StyleChecker.Test.Refactoring.TypeClassParameter;

using System;

public sealed class Okay
{
    public class IncludesStaticClass
    {
        public void Print(Type t)
        {
            Console.WriteLine(t.FullName);
        }

        public void Invoke()
        {
            Print(typeof(string));
            Print(typeof(StaticClass));
        }
    }

    public class NotAllArgumentsAreTypeof
    {
        public void Print(Type t)
        {
            Console.WriteLine(t.FullName);
        }

        public void Invoke()
        {
            Print(typeof(string));
            Print(GetType());
        }
    }

    public void NobodyInvokes(Type type)
    {
        Console.WriteLine(type.FullName);
    }

    public class MethodReference
    {
        public string ToName(Type t)
            => t.FullName;

        void Print(Func<Type, string> toName, Type[] array)
        {
            foreach (var i in array)
            {
                Console.Write(toName(i));
            }
        }

        public void Invoke()
        {
            var name = ToName(typeof(object));
            Print(ToName, [typeof(string), typeof(int)]);
        }
    }

    public class InstanceMethodReference
    {
        public string ToName(Type t)
            => t.FullName;

        public void Invoke()
        {
            var name = ToName(typeof(object));
        }
    }

    private Func<Type, string> instanceAction
        = new InstanceMethodReference().ToName;

    public class StaticMethodReference
    {
        public static string ToName(Type t)
            => t.FullName;

        public void Invoke()
        {
            var name = ToName(typeof(object));
        }
    }

    private Func<Type, string> staticAction = StaticMethodReference.ToName;

    public static class StaticClass;
}
