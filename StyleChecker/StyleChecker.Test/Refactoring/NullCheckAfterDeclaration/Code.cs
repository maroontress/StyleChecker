#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

using System;
using System.IO;

public sealed class Code
{
    public static void FlowStateToExitIfItIsNull()
    {
        var file = Foo();
        //@ ^file
        if (file is null)
        {
            throw new Exception();
        }
        _ = File.ReadAllText(file);
    }

    public static void FlowStateToAssignIfItIsNull()
    {
        var file = Foo();
        //@ ^file
        if (file is null)
        {
            file = "default.txt";
        }
        _ = File.ReadAllText(file);
    }

    public static void IsNull()
    {
        var implicitVar = Foo();
        //@ ^implicitVar
        if (implicitVar is null)
        {
            // implicitVar is null
        }
        string? explicitVar = Foo();
        //@     ^explicitVar
        if (explicitVar is null)
        {
            // explicitVar is null
        }
    }

    public static void IsNotEmptyClause()
    {
        var implicitVar = Foo();
        //@ ^implicitVar
        if (implicitVar is not {})
        {
            // implicitVar is null
        }
        string? explicitVar = Foo();
        //@     ^explicitVar
        if (explicitVar is not {})
        {
            // explicitVar is null
        }
    }

    public static void EqualToNull()
    {
        var implicitVar = Foo();
        //@ ^implicitVar
        if (implicitVar == null)
        {
            // implicitVar is null
        }
        string? explicitVar = Foo();
        //@     ^explicitVar
        if (explicitVar == null)
        {
            // explicitVar is null
        }
    }

    public static void IsNotNull()
    {
        var implicitVar = Foo();
        //@ ^implicitVar
        if (implicitVar is not null)
        {
            // implicitVar is not null
        }
        string? explicitVar = Foo();
        //@     ^explicitVar
        if (explicitVar is not null)
        {
            // explicitVar is not null
        }
    }

    public static void IsEmptyClause()
    {
        var implicitVar = Foo();
        //@ ^implicitVar
        if (implicitVar is {})
        {
            // implicitVar is not null
        }
        string? explicitVar = Foo();
        //@     ^explicitVar
        if (explicitVar is {})
        {
            // explicitVar is not null
        }
    }

    public static void NotEqualToNull()
    {
        var implicitVar = Foo();
        //@ ^implicitVar
        if (implicitVar != null)
        {
            // implicitVar is null
        }
        string? explicitVar = Foo();
        //@     ^explicitVar
        if (explicitVar != null)
        {
            // explicitVar is null
        }
    }

    public static void MultipleDeclarators()
    {
        string? foo = Foo(), bar = Foo();
        //@                  ^bar
        if (bar is null)
        {
        }
    }

    public static void TriviaWithExplicitVar()
    {
        /*A*/ string? /*B*/ foo /*C*/ = /*D*/ Foo() /*E*/; // F
        //@                 ^foo
        /*G*/ if (/*H*/ foo /*I*/ is /*J*/ null /*K*/) // L
        {
        }
    }

    public static void TriviaWithExplicitVarAndMultipleDeclarators()
    {
        /*A*/ string? /*B*/ foo /*C*/ = /*D*/ Foo() /*E*/, /*F*/ bar /*G*/ = /*H*/ Foo() /*I*/; // J
        //@                                                      ^bar
        /*K*/ if (/*L*/ bar /*M*/ is /*N*/ null /*O*/) // P
        {
        }
    }

    public static void TriviaWithImplicitVar()
    {
        /*A*/ var /*B*/ foo /*C*/ = /*D*/ Foo() /*E*/; // F
        //@             ^foo
        /*G*/ if (/*H*/ foo /*I*/ is /*J*/ null /*K*/) // L
        {
        }
    }

    private static string? Foo() => Environment.GetEnvironmentVariable("FILE");
}
