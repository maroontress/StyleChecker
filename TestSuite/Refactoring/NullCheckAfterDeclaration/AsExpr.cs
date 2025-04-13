#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

public sealed class AsExpr
{
    public void TreatAsExpressionSpecially(object o)
    {
        var implicitVar = o as string;
        //@ ^implicitVar
        if (implicitVar is not null)
        {
            _ = implicitVar;
        }
        string? explicitVar = o as string;
        //@     ^explicitVar
        if (explicitVar is not null)
        {
            _ = explicitVar;
        }
    }

    public void InsideParentheses(object o)
    {
        var implicitVar = (o as string);
        //@ ^implicitVar
        if (implicitVar is not null)
        {
            _ = implicitVar;
        }
        string? explicitVar = (o as string);
        //@     ^explicitVar
        if (explicitVar is not null)
        {
            _ = explicitVar;
        }
    }

    public static void TriviaWithExplicitVar(object o)
    {
        /*A*/ string? /*B*/ foo /*C*/ = /*D*/ o /*E*/ as /*F*/ string /*G*/; // H
        //@                 ^foo
        /*I*/ if (/*J*/ foo /*K*/ is /*L*/ null /*M*/) // N
        {
        }
    }

    public static void TriviaWithImplicitVar(object o)
    {
        /*A*/ var /*B*/ foo /*C*/ = /*D*/ o /*E*/ as /*F*/ string /*G*/; // H
        //@             ^foo
        /*I*/ if (/*J*/ foo /*K*/ is /*L*/ null /*M*/) // N
        {
        }
    }

    public static void TriviaWithParentheses(object o)
    {
        /*A*/ var /*B*/ foo /*C*/ = /*D*/ (/*_E_*/ o /*F*/ as /*G*/ string /*H*/) /*_I_*/; // J
        //@             ^foo
        /*K*/ if (/*L*/ foo /*M*/ is /*N*/ null /*O*/) // P
        {
        }
    }
}
