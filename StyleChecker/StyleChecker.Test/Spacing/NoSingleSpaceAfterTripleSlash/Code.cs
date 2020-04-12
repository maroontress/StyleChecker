namespace StyleChecker.Test.Spacing.NoSingleSpaceAfterTripleSlash
{
    public sealed class Code
    {
        ///<summary>
//@     ^
        ///  summary with extra indent.
        ///</summary>
//@0
        ///  <param name="x">first parameter.</param>
//@0
        ///   <param name="y">second parameter.</param>
//@0
        /// <remarks>
        ///remarks. 
//@0
        /// </remarks>
        private void ExtraIndent(int x, int y)
        {
        }

        /// <summary>
        /// summary.
        /// </summary>
        /// <remarks>
        ///<ul>
//@0
        ///<li>NG</li>
//@0
        /// <li>Okay</li>
        ///  <li>Okay</li>
        ///</ul>
//@0
        /// </remarks>
        private void NestedXmlCanHaveExtraIndent()
        {
        }

        /// <summary>
        /// summary
        /// </summary>
        /// <param name="a">
        /// first parameter
        /// </param>
        /// <param name="b">
        /// second parameter
        /// </param>
        /// <seealso
        /// cref="LineBreakInsideAttribute(string, string)"/>
        /// <seealso cref
        /// ="LineBreakInsideAttribute(string, string)"/>
        /// <seealso cref=
        /// "LineBreakInsideAttribute(string, string)"/>
        /// <seealso cref="
        /// LineBreakInsideAttribute(string, string)"/>
        /// <seealso cref="LineBreakInsideAttribute
        /// (string, string)"/>
        /// <seealso cref="LineBreakInsideAttribute(
        /// string, string)"/>
        /// <seealso cref="LineBreakInsideAttribute(string,
        /// string)"/>
        /// <seealso cref="LineBreakInsideAttribute(string, string
        /// )"/>
        /// <seealso cref="LineBreakInsideAttribute(string, string)
        /// "/>
        /// <seealso cref="LineBreakInsideAttribute(string, string)"
        /// />
        private void LineBreakInsideAttribute(string a, string b)
        {
        }

        /// <summary>
        /// summary
        /// </summary>
        /// <seealso
        ///  cref="LineBreakInsideAttribute(string, string)"/>
//@0
        /// <seealso cref="LineBreakInsideAttribute(string,
        ///string)"/>
//@0
        private void LineBreakInsideAttributeFix()
        {
        }
    }
}
