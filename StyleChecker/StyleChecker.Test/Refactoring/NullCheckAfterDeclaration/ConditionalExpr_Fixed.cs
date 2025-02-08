#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

public sealed class ConditionalExpr
{
    public void M(int x, int y)
    {

        if (((x < y) ? X : (A?)Y) is
            {
            } implicitVar)
        {
            _ = implicitVar;
        }

        if ((A?)((x < y) ? X : Y) is A explicitVar)
        {
            _ = explicitVar;
        }
    }

    public B? X { get; set; }

    public C? Y { get; set; }
}

public class A
{
}

public sealed class B : A
{
}

public sealed class C : A
{
}
