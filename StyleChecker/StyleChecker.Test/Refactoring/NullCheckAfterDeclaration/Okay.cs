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

    public static void RecursivePatternHasDesignation()
    {
        var maybeFile = Environment.GetEnvironmentVariable("FILE");
        if (maybeFile is {} file)
        {
        }
    }

    public static void RecursivePatternHasProperty()
    {
        var maybeFile = Environment.GetEnvironmentVariable("FILE");
        if (maybeFile is { Length: > 0 })
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

    public static void InitialValueIsNonNullableValueType()
    {
        int? i = 0;
        if (i is not null)
        {
            _ = i.Value;
        }

#pragma warning disable CS0472
        int a = 0;
        if (a != null)
        {
        }
#pragma warning restore CS0472
    }
}
