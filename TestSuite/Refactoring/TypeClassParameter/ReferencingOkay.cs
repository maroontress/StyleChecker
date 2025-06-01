namespace StyleChecker.Test.Refactoring.TypeClassParameter;

public sealed class ReferencingOkay
{
    public void CallWithStringType()
    {
        ReferencedOkay.PrintMethod(typeof(string));
    }

    public void CallWithIntType()
    {
        ReferencedOkay.PrintMethod(typeof(int));
    }

    public void MethodReference()
    {
        var method = ReferencedOkay.PrintMethod;
    }
}
