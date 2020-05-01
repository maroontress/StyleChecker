namespace StyleChecker.Test.Document.StrayText
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
        private void HasSummary()
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
    }
}
