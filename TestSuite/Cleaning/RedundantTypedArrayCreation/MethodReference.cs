namespace StyleChecker.Test.Cleaning.RedundantTypedArrayCreation;

using System;

public sealed class MethodReference
{
    public void FromCSharp10()
    {
        var all = new Action[]
        //@           ^Action[]|[]
        {
            One,
            Two,
        };
    }

    public void NewAndNull()
    {
        int? Foo() => 42;

        var all = new Func<int?>[]
        //@           ^Func<int?>[]|[]
        {
            Foo,
            new(() => null),
            null,
        };
    }

    public void Delegate()
    {
        var all = new Action[]
        //@           ^Action[]|[]
        {
            (Action)One,
            Two,
        };
    }

    public void Null()
    {
        var all = new Action[]
        //@           ^Action[]|[]
        {
            One,
            (Action)null,
        };
    }

    private void One()
    {
    }

    private void Two()
    {
    }
}
