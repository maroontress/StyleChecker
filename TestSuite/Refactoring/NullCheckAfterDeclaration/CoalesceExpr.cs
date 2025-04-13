#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

public sealed class CoalesceExpr
{
    public void InsideParentheses(string? s, string? d)
    {
        string? explicitVar = (s ?? d);
        //@     ^explicitVar
        if (explicitVar is not null)
        {
            _ = explicitVar;
        }
    }

    public void NeedToParenthesize(string? s, string? d)
    {
        var implicitVar = s ?? d;
        //@ ^implicitVar
        if (implicitVar is not null)
        {
            _ = implicitVar;
        }
        string? explicitVar = s ?? d;
        //@     ^explicitVar
        if (explicitVar is not null)
        {
            _ = explicitVar;
        }
    }
}
