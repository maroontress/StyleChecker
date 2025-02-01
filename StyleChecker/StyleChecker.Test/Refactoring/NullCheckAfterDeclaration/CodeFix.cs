#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

using System;

public sealed class Code
{
    public static void IsNull()
    {
        if (Environment.GetEnvironmentVariable("FILE") is not
            {
            } implicitVar)
        {
            // implicitVar is null
        }
        if (Environment.GetEnvironmentVariable("FILE") is not string explicitVar)
        {
            // explicitVar is null
        }
    }

    public static void IsNotEmptyClause()
    {
        if (Environment.GetEnvironmentVariable("FILE") is not
            {
            } implicitVar)
        {
            // implicitVar is null
        }
        if (Environment.GetEnvironmentVariable("FILE") is not string explicitVar)
        {
            // explicitVar is null
        }
    }

    public static void EqualToNull()
    {
        if (Environment.GetEnvironmentVariable("FILE") is not
            {
            } implicitVar)
        {
            // implicitVar is null
        }
        if (Environment.GetEnvironmentVariable("FILE") is not string explicitVar)
        {
            // explicitVar is null
        }
    }

    public static void IsNotNull()
    {
        if (Environment.GetEnvironmentVariable("FILE") is
            {
            } implicitVar)
        {
            // implicitVar is not null
        }
        if (Environment.GetEnvironmentVariable("FILE") is string explicitVar)
        {
            // explicitVar is not null
        }
    }

    public static void IsEmptyClause()
    {
        if (Environment.GetEnvironmentVariable("FILE") is
            {
            } implicitVar)
        {
            // implicitVar is not null
        }
        if (Environment.GetEnvironmentVariable("FILE") is string explicitVar)
        {
            // explicitVar is not null
        }
    }

    public static void NotEqualToNull()
    {
        if (Environment.GetEnvironmentVariable("FILE") is
            {
            } implicitVar)
        {
            // implicitVar is null
        }
        if (Environment.GetEnvironmentVariable("FILE") is string explicitVar)
        {
            // explicitVar is null
        }
    }
}
