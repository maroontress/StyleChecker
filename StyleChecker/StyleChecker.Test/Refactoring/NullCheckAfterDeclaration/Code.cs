#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

using System;

public sealed class Code
{
    public static void IsNull()
    {
        var implicitVar = Environment.GetEnvironmentVariable("FILE");
        //@ ^implicitVar
        if (implicitVar is null)
        {
            // implicitVar is null
        }
        string? explicitVar = Environment.GetEnvironmentVariable("FILE");
        //@     ^explicitVar
        if (explicitVar is null)
        {
            // explicitVar is null
        }
    }

    public static void IsNotEmptyClause()
    {
        var implicitVar = Environment.GetEnvironmentVariable("FILE");
        //@ ^implicitVar
        if (implicitVar is not {})
        {
            // implicitVar is null
        }
        string? explicitVar = Environment.GetEnvironmentVariable("FILE");
        //@     ^explicitVar
        if (explicitVar is not {})
        {
            // explicitVar is null
        }
    }

    public static void EqualToNull()
    {
        var implicitVar = Environment.GetEnvironmentVariable("FILE");
        //@ ^implicitVar
        if (implicitVar == null)
        {
            // implicitVar is null
        }
        string? explicitVar = Environment.GetEnvironmentVariable("FILE");
        //@     ^explicitVar
        if (explicitVar == null)
        {
            // explicitVar is null
        }
    }

    public static void IsNotNull()
    {
        var implicitVar = Environment.GetEnvironmentVariable("FILE");
        //@ ^implicitVar
        if (implicitVar is not null)
        {
            // implicitVar is not null
        }
        string? explicitVar = Environment.GetEnvironmentVariable("FILE");
        //@     ^explicitVar
        if (explicitVar is not null)
        {
            // explicitVar is not null
        }
    }

    public static void IsEmptyClause()
    {
        var implicitVar = Environment.GetEnvironmentVariable("FILE");
        //@ ^implicitVar
        if (implicitVar is {})
        {
            // implicitVar is not null
        }
        string? explicitVar = Environment.GetEnvironmentVariable("FILE");
        //@     ^explicitVar
        if (explicitVar is {})
        {
            // explicitVar is not null
        }
    }

    public static void NotEqualToNull()
    {
        var implicitVar = Environment.GetEnvironmentVariable("FILE");
        //@ ^implicitVar
        if (implicitVar != null)
        {
            // implicitVar is null
        }
        string? explicitVar = Environment.GetEnvironmentVariable("FILE");
        //@     ^explicitVar
        if (explicitVar != null)
        {
            // explicitVar is null
        }
    }
}
