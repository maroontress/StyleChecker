#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass
{
    /// <summary>
    /// Class summary.
    /// </summary>
    /// <typeparam name="T">
    /// Description for T.
    /// </typeparam>
    /// <typeparam name="U">
    /// Description for U.
    /// </typeparam>
    public static class MultiTypeParamCode<T, U>
        where T : class
        where U : class
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
        public static void Method(T first, U second)
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
         */
        public static void TheOtherStyleComment(T first, U second)
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
        */
        public static void NoLeadingAsteriskStyleComment(T first, U second)
        {
        }

        public static void NoDocumentationComment(T first, U second)
        {
        }
    }

    /**
     * <summary>
     * Class summary.
     * </summary>
     * <typeparam name="T">
     * Description for T.
     * </typeparam>
     * <typeparam name="U">
     * Description for U.
     * </typeparam>
     */
    public static class MldcStyleMultiTypeParamCode<T, U>
        where T : class
        where U : class
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
         */
        public static void Method(T first, U second)
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
        public static void TheOtherStyleComment(T first, U second)
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
        */
        public static void NoLeadingAsteriskStyleComment(T first, U second)
        {
        }

        public static void NoDocumentationComment(T first, U second)
        {
        }
    }
}
