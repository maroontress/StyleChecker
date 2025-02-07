#pragma warning disable CS0693

namespace Application;

using System;

public sealed class Okay<T>
{
    public Okay(T obj)
    {
        _ = obj;
    }

    public T SameNameAsTypeParameterOfOuterType<T>(T obj)
    {
        return obj;
    }

    public bool UseTypeTInsideAndUseAnotherTypeToo<U>(T t, U u)
    {
        return ReferenceEquals(t, u);
    }

    public static T OK<T>(T obj)
    {
        return obj;
    }

    public static TValue GetValue<TKey, TValue>(TKey key, Func<TKey, TValue> map)
    {
        return map(key);
    }

    public static @T Verbatim<@T>(@T obj)
    {
        return obj;
    }
}

public class T<U>
{
}

public sealed class Outer<T>
{
    public void M<U>(T t, U u)
    {
        _ = t;
        _ = u;
    }

    public sealed class Inner<U>
    {
        public Inner(T t, U u)
        {
            _ = t;
            _ = u;
        }
    }
}
