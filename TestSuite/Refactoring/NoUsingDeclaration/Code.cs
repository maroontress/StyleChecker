#nullable enable
namespace StyleChecker.Test.Refactoring.NoUsingDeclaration;

using System;
using System.IO;

public sealed class Code
{
    public static void Main()
    {
        var i = new StreamReader("file.txt");
    //@ ^var
        StreamReader j = new("file.txt");
    //@ ^StreamReader
    }

    public static void ExplicitTypeDeclarationsWithMultipleDeclarators()
    {
        StreamReader i1 = new StreamReader("1.txt"), i2 = new StreamReader("2.txt");
    //@ ^StreamReader
        StreamReader j1 = new StreamReader("1.txt"), j2 = new("2.txt");
    //@ ^StreamReader
        StreamReader k1 = new("1.txt"), k2 = new StreamReader("2.txt");
    //@ ^StreamReader
    }

    public static void Using()
    {
        // StringReader 'i' does not need to be disposed
        StringReader i = new("hello");
        // IDisposable 'j' should be disposed
        IDisposable j = new StringReader("world");
    //@ ^IDisposable
    }

    public static void Nullable()
    {
        StreamReader? k = new StreamReader("file.txt");
    //@ ^StreamReader?
    }

    public static void KeepTrivia()
    {
        /*foo*/ var i = new StreamReader("file.txt") /*bar*/ ; // baz
        //@     ^var
    }
}
