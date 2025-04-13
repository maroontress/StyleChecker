namespace StyleChecker.Test.Cleaning.RedundantTypedArrayCreation;

using System;

public sealed class Okay
{
    public void Complex()
    {
        var all = new object[] { "a", 1, };
    }

    public void Empty()
    {
        var all = new string[] { };
    }

    public void Null()
    {
        var all = new string[] { null, };
    }

    public void Default()
    {
        var all = new string[] { default, };
    }

    public void Implicit()
    {
        var all = new[] { "a", };
    }

    public void SpecifiedLength()
    {
        var all = new string[1] { "a", };
    }

    public void TwoDimensionSpecifiedLength()
    {
        var all = new string[2, 2] { { "a", "b", }, { "c", "d", }, };
    }

    public void FixedLengthArray()
    {
        var all = new string[3];
    }

    public void ImplicitNewWithNullableValueType()
    {
        var all = new int?[]
        {
            new(),
            null,
        };
    }

    public void DefaultLiteralWithNullableValueType()
    {
        var all = new int?[]
        {
            default,
            null,
        };
    }

    public void NullWithNullableValueType()
    {
        var all = new int?[]
        {
            42,
            null,
        };
    }

    public void ImplicitNewWithValueType()
    {
        var all = new int[]
        {
            new(),
        };
    }

    public void DefaultLiteralWithValueType()
    {
        var all = new int[]
        {
            default,
        };
    }

    public void MethodReference()
    {
        var all = new Func<int?>[]
        {
            new(() => null),
            null,
        };
    }
}
