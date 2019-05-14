namespace StyleChecker.Test.Spacing.NoSingleSpaceAfterTripleSlash
{
    public sealed class Code
    {
        ///
        private void EmptyOkay()
        {
        }

        /// <summary>
        /// summary.
        /// </summary>
        /// <param name="x">
        /// parameter.
        /// </param>
        private void Okay(int x)
        {
        }

        /// <summary>
        /// </summary>
        ///
        private void LastEmpty()
        {
        }

        ///
        /// <summary>
        /// </summary>
        private void FirstEmpty()
        {
        }

        /// <summary>
        ///
        /// </summary>
        private void IncludeEmpty()
        {
        }

        /// <summary>
        ///  summary with extra indent.
        /// </summary>
        /// <param name="x">first parameter.</param>
        /// <param name="y">second parameter.</param>
        /// <remarks>
        /// remarks. 
        /// </remarks>
        private void ExtraIndent(int x, int y)
        {
        }

        /// <summary>
        /// summary.
        /// </summary>
        /// <remarks>
        /// <ul>
        /// <li>NG</li>
        /// <li>Okay</li>
        ///  <li>Okay</li>
        /// </ul>
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
        /// cref="LineBreakInsideAttribute(string, string)"/>
        /// <seealso cref="LineBreakInsideAttribute(string,
        /// string)"/>
        private void LineBreakInsideAttributeFix()
        {
        }
    }
}
