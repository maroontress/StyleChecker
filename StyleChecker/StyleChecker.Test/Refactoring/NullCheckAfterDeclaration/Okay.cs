#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

using System;
using System.IO;

public sealed class Okay
{
    public static void ThereIsSomethingBetweenDeclarationAndNullCheck()
    {
        var file = Environment.GetEnvironmentVariable("FILE");
        var dir = Environment.GetEnvironmentVariable("DIR");
        if (file is null)
        {
        }
    }

    public static void ExplicitAndMultipleDeclarators()
    {
        string? file = Environment.GetEnvironmentVariable("FILE"),
            dir = Environment.GetEnvironmentVariable("DIR");
        if (file is null)
        {
        }
    }

    public static void Using()
    {
        using StreamReader i = new("file.txt");
        if (i is null)
        {
        }
    }
}
