#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

using System;
using System.Collections.Generic;
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

    public static void InitialValueIsNonNullReference()
    {
        //  To begin with, it does not have to be a nullable type, nor does it
        //  have to be null-checked.

        //  Example 1. Collection expression
        IEnumerable<string>? foo = ["foo"];
        //  foo is not null here so null check is insanity.
        if (foo is not null)
        {
            _ = foo;
        }
        /*
            The following code causes a compilation error CS9176 so far:

            if (["foo"] is IEnumerable<string> foo)
        */

        //  Example 2. Implicit new expression
        string? bar = new("bar");
        //  bar is not null here so null check is insanity.
        if (bar is not null)
        {
            _ = bar;
        }
        /*
            The following code causes a compilation error CS8754 so far:

            if (new("bar") is string bar)
        */
    }
}
