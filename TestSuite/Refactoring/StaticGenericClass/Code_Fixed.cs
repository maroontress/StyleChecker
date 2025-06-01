#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass;

public static class Code
{
    public static void Method<T>(T instance) where T : class
    {
    }
}

public static class NoConstraintClause
{
    public static void Method<T>(T instance)
    {
    }
}

public static class MultipleTypeParameter
{
    public static void Method<T, U>(T instance, U option) where T : class
        where U : class
    {
    }
}

/// <summary>Class Summary.</summary>
/// 
public static class SingleLineDocumentationComment
{
    /// <summary>Method summary.</summary>
    /// <param name="instance">Parameter.</param>
    /// <typeparam name="T">Class type parameter.</typeparam>
    public static void Method<T>(T instance) where T : class
    {
    }

    /**
     * <summary>Method Summary.</summary>
     * <param name="instance">Parameter.</param>
     * <typeparam name="T">Class type parameter.</typeparam>
     */
    public static void TheOtherStyleComment<T>(T instance) where T : class
    {
    }

    /// <typeparam name="T">Class type parameter.</typeparam>
    public static void NoDocumentationComment<T>(T instance) where T : class
    {
    }
}

/**
    <summary>Class Summary.</summary>

*/
public static class MultiLineDocumentationComment
{
    /**
        <summary>Method summary.</summary>
        <param name="instance">Parameter.</param>
        <typeparam name="T">Class type parameter.</typeparam>
    */
    public static void Method<T>(T instance) where T : class
    {
    }

    /// <summary>Method Summary.</summary>
    /// <param name="instance">Parameter.</param>
    /// <typeparam name="T">Class type parameter.</typeparam>
    public static void TheOtherStyleComment<T>(T instance) where T : class
    {
    }

    /// <typeparam name="T">Class type parameter.</typeparam>
    public static void NoDocumentationComment<T>(T instance) where T : class
    {
    }
}

public static class Reference
{
    public static void Caller()
    {
        Code.Method("hello");
    }
}
