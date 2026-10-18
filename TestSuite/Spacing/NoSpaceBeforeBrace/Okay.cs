#nullable enable
namespace Application;

using System;

public sealed class Okay
{
    public void Basic()
    {
        if (true)
        {
        }
        var array = new[] { "" };
        var array2 = new[] {
            "" };
        var array3 = new[]
        {
            "",
        };
        var array4 = new[] { new[] { "" }, };
        Action doNothing = () => {};
        if (array is { Length: 0 } z)
        {
        }
        if (array is {} e)
        {
        }

        static void M(string[] a)
        {
        }

        M(new[] { "" });
    }

    public void Interpolation(string m)
    {
        _ = $"{m}";
    }

    public void PropertyPatternInParen(object? o)
    {
        var b = o is ({});
        Console.WriteLine(b);
    }

    public void PropertyPatternAndCollectionExpr()
    {
        string?[] list = ["hello, world"];
        if (list is [{} x])
        {
            Console.WriteLine(x);
        }

        string?[] twoElements = ["hello", "world"];
        var b = twoElements is [{}];
        Console.WriteLine(b);
    }
}
