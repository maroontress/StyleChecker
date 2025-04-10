#nullable enable
namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

public sealed class ConditionalExpr
{
    public void InsideParenthesesDifferentType(bool b, B? x, C? y)
    {
        A? explicitVar = ((b) ? x : y);
        //@^explicitVar
        if (explicitVar is not null)
        {
            _ = explicitVar;
        }
    }

    public void InsideParenthesesSameType(bool b, string x, string? y)
    {
        string? explicitVar = ((b) ? x : y);
        //@     ^explicitVar
        if (explicitVar is not null)
        {
            _ = explicitVar;
        }
    }

    public void BothTypesAreSame(bool b, string x, string? y)
    {
        string? explicitVar = (b) ? x : y;
        //@     ^explicitVar
        if (explicitVar is not null)
        {
            _ = explicitVar;
        }
    }

    public void InsertCastExpression(int x, int y)
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
