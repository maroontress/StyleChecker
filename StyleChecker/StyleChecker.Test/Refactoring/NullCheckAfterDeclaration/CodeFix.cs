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

    public static void MultipleDeclarators()
    {
        string? foo = Foo();
        if (Foo() is not string bar)
        {
        }
    }

    public static void TriviaWithExplicitVar()
    {
        /*A*/
        // F
        /*G*/
        if (/*H*/  /*D*/ Foo() /*E*/is not string /*B*/ foo /*C*/  /*I*/  /*J*/  /*K*/) // L
        {
        }
    }

    public static void TriviaWithExplicitVarAndMultipleDeclarators()
    {
        /*A*/
        string? /*B*/ foo /*C*/ = /*D*/ Foo() /*E*/; // J
        /*K*/
        if (/*L*/  /*H*/ Foo() /*I*/is not string /*F*/ bar /*G*/  /*M*/  /*N*/  /*O*/) // P
        {
        }
    }

    public static void TriviaWithImplicitVar()
    {
        /*A*/
        // F
        /*G*/
        if (/*H*/  /*D*/ Foo() /*E*/is not
            {
            }
/*B*/ foo /*C*/  /*I*/  /*J*/  /*K*/) // L
        {
        }
    }

    private static string? Foo() => Environment.GetEnvironmentVariable("FILE");
}
