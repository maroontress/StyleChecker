#pragma warning disable CS0693

namespace Application
{
    public sealed class Code<T>
    {
        public Code(T obj)
        {
        }

        public static T OK<T>(T obj)
        {
            return obj;
        }
    }
}
