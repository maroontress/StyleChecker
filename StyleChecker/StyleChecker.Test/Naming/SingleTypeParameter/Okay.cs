#pragma warning disable CS0693

namespace Application
{
    using System;

    public sealed class Okay<T>
    {
        public Okay(T obj)
        {
        }

        public static T OK<T>(T obj)
        {
            return obj;
        }

        public static TValue GetValue<TKey, TValue>(TKey key, Func<TKey, TValue> map)
        {
            return map(key);
        }
    }
}
