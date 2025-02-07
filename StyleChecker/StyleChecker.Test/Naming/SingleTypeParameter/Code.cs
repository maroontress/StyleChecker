namespace Application;

public sealed class Code<Type>
    //@                  ^Type
{
    public Code(Type obj)
    {
    }
}

public sealed class ContainingType
{
    public static Type NG<Type>(Type obj)
    //@                   ^Type
    {
        return obj;
    }
}
