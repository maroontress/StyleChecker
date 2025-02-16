namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

using System;
using System.Collections.Generic;

public sealed class TypeInferenceOkay
{
    public static void CollectionExpression()
    {
        IEnumerable<string> foo = ["foo"];
        //  foo is not null here so null check is insanity.
        if (foo is not null)
        {
            _ = foo;
        }
        /*
            The following code causes a compilation error CS9176 so far:

            if (["foo"] is IEnumerable<string> foo)
        */
    }

    public static void ImplicitNewExpression()
    {
        string foo = new("foo");
        //  foo is not null here so null check is insanity.
        if (foo is not null)
        {
            _ = foo;
        }
        /*
            The following code causes a compilation error CS8754 so far:

            if (new("foo") is string foo)
        */
    }

    public static void ImplicitStackAllocArrayExpression()
    {
        Span<int> foo = stackalloc[] { 1 };
        //  foo is not null here so null check is insanity.
        if (foo == null)
        {
        }
        /*
            The following code causes a compilation error CS8518 so far:

            if (stackalloc[] { 1 } is not Span<int> foo)
        */
    }

    public static void DefaultExpression()
    {
        string foo = default;
        //  foo is null here so null check is insanity.
        if (foo is not null)
        {
            _ = foo;
        }
        /*
            The following code causes a compilation error CS8716 so far:

            if (default is string foo)
        */
    }

    public static void NullExpression()
    {
        string foo = null;
        //  foo is null here so null check is insanity.
        if (foo is not null)
        {
            _ = foo;
        }
        /*
            The following code causes a compilation error CS8117 so far:

            if (null is string foo)
        */
    }
}
