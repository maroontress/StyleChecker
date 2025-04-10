namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

using System;
using System.IO;

public sealed class Code
{
    public static void InitialValueIsIdentifier(string foo)
    {

        if (foo is
            {
            } bar)
        {
        }
    }

    public static void FlowState_IfNull_ThrowInsideThen_ReadAfterIf()
    {

        if (Foo() is not
            {
            } file)
        {
            throw new Exception();
        }
        _ = File.ReadAllText(file);
    }

    public static void FlowState_IfNull__ReturnInsideThen_ReadAfterIf()
    {

        if (Foo() is not
            {
            } file)
        {
            return;
        }
        _ = File.ReadAllText(file);
    }

    public static void FlowState_IfNull_AlwaysAssignInsideThen_ReadAfterIf()
    {

        if (Foo() is not
            {
            } file)
        {
            file = "default.txt";
        }
        _ = File.ReadAllText(file);
    }

    public static void FlowState_IfNull_ReadInsideThen_AlwaysAssignInsideThen()
    {

        if (Foo() is not
            {
            } file)
        {
            file = "default.txt";
            _ = File.ReadAllText(file);
        }
    }

    public static void FlowState_IfNull()
    {

        if (Foo() is not
            {
            } file)
        {
        }
    }

    public static void FlowState_IfNotNull()
    {

        if (Foo() is
            {
            } file)
        {
        }
    }

    public static void IsNull()
    {

        if (Foo() is not
            {
            } implicitVar)
        {
            // implicitVar is null
        }

        if (Foo() is not string explicitVar)
        {
            // explicitVar is null
        }
    }

    public static void IsNotEmptyClause()
    {

        if (Foo() is not
            {
            } implicitVar)
        {
            // implicitVar is null
        }

        if (Foo() is not string explicitVar)
        {
            // explicitVar is null
        }
    }

    public static void EqualToNull()
    {

        if (Foo() is not
            {
            } implicitVar)
        {
            // implicitVar is null
        }

        if (Foo() is not string explicitVar)
        {
            // explicitVar is null
        }
    }

    public static void IsNotNull()
    {

        if (Foo() is
            {
            } implicitVar)
        {
            // implicitVar is not null
        }

        if (Foo() is string explicitVar)
        {
            // explicitVar is not null
        }
    }

    public static void IsEmptyClause()
    {

        if (Foo() is
            {
            } implicitVar)
        {
            // implicitVar is not null
        }

        if (Foo() is string explicitVar)
        {
            // explicitVar is not null
        }
    }

    public static void NotEqualToNull()
    {

        if (Foo() is
            {
            } implicitVar)
        {
            // implicitVar is null
        }

        if (Foo() is string explicitVar)
        {
            // explicitVar is null
        }
    }

    public static void MultipleDeclarators()
    {
        string foo = Foo();
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
        string /*B*/ foo /*C*/ = /*D*/ Foo() /*E*/; // J
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

    private static string Foo() => Environment.GetEnvironmentVariable("FILE");
}
