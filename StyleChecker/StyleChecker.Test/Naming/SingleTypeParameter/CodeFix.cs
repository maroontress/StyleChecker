namespace Application;

public sealed class Code<T>
{
    public Code(T obj)
    {
    }
}

public sealed class ContainingType
{
    public static T NG<T>(T obj)
    {
        return obj;
    }
}
