#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

public sealed class ConditionalExpr
{
    public void M(int x, int y)
    {
        var implicitVar = (x < y) ? X : (A?)Y;
        //@ ^implicitVar
        if (implicitVar is not null)
        {
            _ = implicitVar;
        }
        A? explicitVar = (x < y) ? X : Y;
        //@^explicitVar
        if (explicitVar is not null)
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
