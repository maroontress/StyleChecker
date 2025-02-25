#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

public sealed class AsExpr
{
    public void TreatAsExpressionSpecially(object o)
    {

        if (o is string implicitVar)
        {
            _ = implicitVar;
        }

        if (o is string explicitVar)
        {
            _ = explicitVar;
        }
    }

    public void InsideParentheses(object o)
    {

        if (o is string implicitVar)
        {
            _ = implicitVar;
        }

        if (o is string explicitVar)
        {
            _ = explicitVar;
        }
    }

    public static void TriviaWithExplicitVar(object o)
    {
        /*A*/
        // H
        /*I*/
        if (/*J*/  /*D*/ o /*E*/ is not
/*F*/ string /*G*/ /*B*/ foo /*C*/  /*K*/  /*L*/  /*M*/) // N
        {
        }
    }

    public static void TriviaWithImplicitVar(object o)
    {
        /*A*/
        // H
        /*I*/
        if (/*J*/  /*D*/ o /*E*/ is not
/*F*/ string /*G*/ /*B*/ foo /*C*/  /*K*/  /*L*/  /*M*/) // N
        {
        }
    }

    public static void TriviaWithParentheses(object o)
    {
        /*A*/
        // J
        /*K*/
        if (/*L*/  /*D*/ o /*F*/ /*_E_*/  /*_I_*/is not
/*G*/ string /*H*/ /*B*/ foo /*C*/  /*M*/  /*N*/  /*O*/) // P
        {
        }
    }
}
