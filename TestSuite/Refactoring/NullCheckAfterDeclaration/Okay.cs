#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

using System;
using System.IO;

public sealed class Okay
{
    public static void FlowState_IfNull_ReadInsideThen()
    {
        var file = Environment.GetEnvironmentVariable("FILE");
        if (file is null)
        {
            // XXX
#pragma warning disable CS8604
            // warning CS8604: Possible null reference argument for parameter 'path' in 'File.ReadAllText(string path)'
            _ = File.ReadAllText(file);
#pragma warning restore CS8604
        }
    }

    public static void FlowState_IfNull_ReadAfterIf()
    {
        var file = Environment.GetEnvironmentVariable("FILE");
        if (file is null)
        {
        }
        // XXX
#pragma warning disable CS8604
        // warning CS8604: Possible null reference argument for parameter 'path' in 'File.ReadAllText(string path)'
        _ = File.ReadAllText(file);
#pragma warning restore CS8604
    }

    public static void FlowState_IfNull_Else_ReadInsideThen()
    {
        var file = Environment.GetEnvironmentVariable("FILE");
        if (file is null)
        {
            // XXX
#pragma warning disable CS8604
            // warning CS8604: Possible null reference argument for parameter 'path' in 'File.ReadAllText(string path)'
            _ = File.ReadAllText(file);
#pragma warning restore CS8604
        }
        else
        {
            _ = File.ReadAllText(file);
        }
    }

    public static void FlowState_IfNull_Else_ReadAfterIf()
    {
        var file = Environment.GetEnvironmentVariable("FILE");
        if (file is null)
        {
        }
        else
        {
            _ = File.ReadAllText(file);
        }
        // XXX
#pragma warning disable CS8604
        // warning CS8604: Possible null reference argument for parameter 'path' in 'File.ReadAllText(string path)'
        _ = File.ReadAllText(file);
#pragma warning restore CS8604
    }

    public static void FlowState_IfNotNull_ReadAfterIf()
    {
        var file = Environment.GetEnvironmentVariable("FILE");
        if (file is not null)
        {
            _ = File.ReadAllText(file);
        }
        // XXX
#pragma warning disable CS8604
        // warning CS8604: Possible null reference argument for parameter 'path' in 'File.ReadAllText(string path)'
        _ = File.ReadAllText(file);
#pragma warning restore CS8604
    }

    public static void FlowState_IfNotNull_Else_ReadInsideElse()
    {
        var file = Environment.GetEnvironmentVariable("FILE");
        if (file is not null)
        {
            _ = File.ReadAllText(file);
        }
        else
        {
            // XXX
#pragma warning disable CS8604
            // warning CS8604: Possible null reference argument for parameter 'path' in 'File.ReadAllText(string path)'
            _ = File.ReadAllText(file);
#pragma warning restore CS8604
        }
    }

    public static void FlowState_IfNotNull_Else_ReadAfterIf()
    {
        var file = Environment.GetEnvironmentVariable("FILE");
        if (file is not null)
        {
            _ = File.ReadAllText(file);
        }
        else
        {
        }
        // XXX
#pragma warning disable CS8604
        // warning CS8604: Possible null reference argument for parameter 'path' in 'File.ReadAllText(string path)'
        _ = File.ReadAllText(file);
#pragma warning restore CS8604
    }

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
        var foo = File.ReadAllText("default.txt");
        if (foo is not null)
        {
            _ = foo;
        }
    }

    public static void NullLiteral()
    {
        string? foo = null;
        if (foo is not null)
        {
            _ = foo;
        }
    }

    public static void NullLiteralWithAsExpression()
    {
        var foo = null as string;
        if (foo is not null)
        {
            _ = foo;
        }
    }
}
