#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

public sealed class CoalesceExpr
{
    public void InsideParentheses(string? s, string? d)
    {

        if ((s ?? d) is string explicitVar)
        {
            _ = explicitVar;
        }
    }

    public void NeedToParenthesize(string? s, string? d)
    {

        if ((s ?? d) is
            {
            } implicitVar)
        {
            _ = implicitVar;
        }

        if ((s ?? d) is string explicitVar)
        {
            _ = explicitVar;
        }
    }
}
