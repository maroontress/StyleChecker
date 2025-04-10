namespace StyleChecker.Test.Spacing.NoSingleSpaceAfterTripleSlash
{
    public sealed class Code
    {
        ///<summary>
//@        ^
        ///  summary with extra indent.
        ///</summary>
//@        ^
        ///  <param name="x">first parameter.</param>
//@        ^
        ///   <param name="y">second parameter.</param>
//@        ^
        /// <remarks>
        ///remarks. 
//@        ^
        /// </remarks>
        private void ExtraIndent(int x, int y)
        {
        }

        /// <summary>
        /// summary.
        /// </summary>
        /// <remarks>
        ///<ul>
//@        ^
        ///<li>NG</li>
//@        ^
        /// <li>Okay</li>
        ///  <li>Okay</li>
        ///</ul>
//@        ^
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
//@          ^
        /// <seealso cref="LineBreakInsideAttribute(string,
        ///string)"/>
//@        ^
        private void LineBreakInsideAttributeFix()
        {
        }

        public void OutsideXml()
        {
            ///hello
//@            ^

            ///<![CDATA[hoge]]>
//@            ^
        }

        /// <summary>
        ///<![CDATA[
//@        ^
        /// Hello, World!
        /// ]]>
        /// </summary>
        public void CDataSectionBegin()
        {
        }

        /// <summary>
        ///  <![CDATA[
        ///Hello, World!
//@        ^
        /// ]]>
        /// </summary>
        public void InsideCDataSection()
        {
        }

        /// <summary>
        /// <![CDATA[
        /// Hello, World!
        ///]]>
//@        ^
        /// </summary>
        public void CDataSectionEnd()
        {
        }
    }
}
