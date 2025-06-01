#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass;

public static class Code<T>
    //@             ^Code
    where T : class
{
    public static void Method(T instance)
    {
    }
}

public static class NoConstraintClause<T>
    //@             ^NoConstraintClause
{
    public static void Method(T instance)
    {
    }
}

public static class MultipleTypeParameter<T>
    //@             ^MultipleTypeParameter
    where T : class
{
    public static void Method<U>(T instance, U option)
        where U : class
    {
    }
}

/// <summary>Class Summary.</summary>
/// <typeparam name="T">Class type parameter.</typeparam>
public static class SingleLineDocumentationComment<T>
    //@             ^SingleLineDocumentationComment
    where T : class
{
    /// <summary>Method summary.</summary>
    /// <param name="instance">Parameter.</param>
    public static void Method(T instance)
    {
    }

    /**
     * <summary>Method Summary.</summary>
     * <param name="instance">Parameter.</param>
     */
    public static void TheOtherStyleComment(T instance)
    {
    }

    public static void NoDocumentationComment(T instance)
    {
    }
}

/**
    <summary>Class Summary.</summary>
    <typeparam name="T">Class type parameter.</typeparam>
*/
public static class MultiLineDocumentationComment<T>
    //@             ^MultiLineDocumentationComment
    where T : class
{
    /**
        <summary>Method summary.</summary>
        <param name="instance">Parameter.</param>
    */
    public static void Method(T instance)
    {
    }

    /// <summary>Method Summary.</summary>
    /// <param name="instance">Parameter.</param>
    public static void TheOtherStyleComment(T instance)
    {
    }

    public static void NoDocumentationComment(T instance)
    {
    }
}

public static class Reference
{
    public static void Caller()
    {
        Code<string>.Method("hello");
    }
}
