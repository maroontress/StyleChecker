namespace StyleChecker.Test.Spacing.NoSingleSpaceAfterTripleSlash
{
    public sealed class Okay
    {
        ///
        private void Empty()
        {
        }

        /// <summary>
        /// summary.
        /// </summary>
        /// <param name="x">
        /// parameter.
        /// </param>
        private void NoProblem(int x)
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

        /// Hello
        private void NoXml()
        {
        }

        ///  Hello
        private void NoXmlExtraIndent()
        {
        }

        /// header
        /// <summary>Hello, world</summary>
        /// footer
        private void OutSideXml()
        {
        }

        ///  header
        /// <summary>Hello, world</summary>
        ///  footer
        private void OutSideXmlExtraIndent()
        {
        }

        /// <summary>
        ///	header
        /// </summary>
        private void Tab()
        {
        }

        ///	header
        private void NoXmlTab()
        {
        }

        /// <summary>
        /// head <see cref="Okay"/> foot
        /// </summary>
        private void ContainsSeeElementInsideSummary()
        {
        }

        /// <summary>
        /// <see cref="Okay"/> foot
        /// </summary>
        private void StartWithSeeElementInsideSummary()
        {
        }

        /// head <see cref="Okay"/> foot
        private void ContainsSeeElement()
        {
        }

        /// <see cref="Okay"/> foot
        private void StartWithSeeElement()
        {
        }

        ///	<summary>
        ///	head <see cref="Okay"/> foot
        ///	</summary>
        private void Tab_ContainsSeeElementInsideSummary()
        {
        }

        ///	<summary>
        ///	<see cref="Okay"/> foot
        ///	</summary>
        private void Tab_StartWithSeeElementInsideSummary()
        {
        }

        ///	head <see cref="Okay"/> foot
        private void Tab_ContainsSeeElement()
        {
        }

        ///	<see cref="Okay"/> foot
        private void Tab_StartWithSeeElement()
        {
        }
    }
}
