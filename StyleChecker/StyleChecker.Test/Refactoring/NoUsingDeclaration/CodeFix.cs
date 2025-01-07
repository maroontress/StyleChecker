#nullable enable
namespace StyleChecker.Test.Refactoring.NoUsingDeclaration;

using System;
using System.IO;

public sealed class Code
{
    public static void Main()
    {
        using var i = new StreamReader("file.txt");
        using StreamReader j = new("file.txt");
    }

    public static void ExplicitTypeDeclarationsWithMultipleDeclarators()
    {
        using StreamReader i1 = new StreamReader("1.txt"), i2 = new StreamReader("2.txt");
        using StreamReader j1 = new StreamReader("1.txt"), j2 = new("2.txt");
        using StreamReader k1 = new("1.txt"), k2 = new StreamReader("2.txt");
    }

    public static void Using()
    {
        // StringReader 'i' does not need to be disposed
        StringReader i = new("hello");
        // IDisposable 'j' should be disposed
        using IDisposable j = new StringReader("world");
    }

    public static void Nullable()
    {
        using StreamReader? k = new StreamReader("file.txt");
    }

    public static void KeepTrivia()
    {
        /*foo*/
        using var i = new StreamReader("file.txt"); // bar
    }
}
