#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass;

/// <summary>
/// Class summary.
/// </summary>
/// 
/// 
public static class MultiTypeParamCode
{
    /// <summary>
    /// Method summary.
    /// </summary>
    /// <param name="first">
    /// The first parameter.
    /// </param>
    /// <param name="second">
    /// The second parameter.
    /// </param>
    /// <typeparam name="T">
    /// Description for T.
    /// </typeparam>
    /// <typeparam name="U">
    /// Description for U.
    /// </typeparam>
    public static void Method<T, U>(T first, U second) where T : class
    where U : class
    {
    }

    /**
     * <summary>
     * Method Summary.
     * </summary>
     * <param name="first">
     * The first parameter.
     * </param>
     * <param name="second">
     * The second parameter.
     * </param>
     * <typeparam name="T">
     * Description for T.
     * </typeparam>
     * <typeparam name="U">
     * Description for U.
     * </typeparam>
     */
    public static void TheOtherStyleComment<T, U>(T first, U second) where T : class
    where U : class
    {
    }

    /**
        <summary>
        Method Summary.
        </summary>
        <param name="first">
        The first parameter.
        </param>
        <param name="second">
        The second parameter.
        </param>
        <typeparam name="T">
         Description for T.
         </typeparam>
        <typeparam name="U">
         Description for U.
         </typeparam>
    */
    public static void NoLeadingAsteriskStyleComment<T, U>(T first, U second) where T : class
    where U : class
    {
    }

    /// <typeparam name="T">
    /// Description for T.
    /// </typeparam>
    /// <typeparam name="U">
    /// Description for U.
    /// </typeparam>
    public static void NoDocumentationComment<T, U>(T first, U second) where T : class
    where U : class
    {
    }
}

/**
 * <summary>
 * Class summary.
 * </summary>
 * 
 * 
 */
public static class MldcStyleMultiTypeParamCode
{
    /**
     * <summary>
     * Method summary.
     * </summary>
     * <param name="first">
     * The first parameter.
     * </param>
     * <param name="second">
     * The second parameter.
     * </param>
     * <typeparam name="T">
     * Description for T.
     * </typeparam>
     * <typeparam name="U">
     * Description for U.
     * </typeparam>
     */
    public static void Method<T, U>(T first, U second) where T : class
    where U : class
    {
    }

    /// <summary>
    /// Method Summary.
    /// </summary>
    /// <param name="first">
    /// The first parameter.
    /// </param>
    /// <param name="second">
    /// The second parameter.
    /// </param>
    /// <typeparam name="T">
    /// Description for T.
    /// </typeparam>
    /// <typeparam name="U">
    /// Description for U.
    /// </typeparam>
    public static void TheOtherStyleComment<T, U>(T first, U second) where T : class
    where U : class
    {
    }

    /**
        <summary>
        Method Summary.
        </summary>
        <param name="first">
        The first parameter.
        </param>
        <param name="second">
        The second parameter.
        </param>
        <typeparam name="T">
         Description for T.
         </typeparam>
        <typeparam name="U">
         Description for U.
         </typeparam>
    */
    public static void NoLeadingAsteriskStyleComment<T, U>(T first, U second) where T : class
    where U : class
    {
    }

    /// <typeparam name="T">
    /// Description for T.
    /// </typeparam>
    /// <typeparam name="U">
    /// Description for U.
    /// </typeparam>
    public static void NoDocumentationComment<T, U>(T first, U second) where T : class
    where U : class
    {
    }
}
