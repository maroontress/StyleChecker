#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

public sealed class CoalesceExpr
{
    public void M(string? s, string? d)
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
