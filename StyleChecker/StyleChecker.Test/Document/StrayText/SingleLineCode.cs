namespace StyleChecker.Test.Document.StrayText
{
    public sealed class Code
    {
        /// Not in the tag.
//@        ^Not in the tag.
        private void NotInTheTag()
        {
        }

        /// Before the tag.
//@        ^Before the tag.
        /// <summary>
        /// </summary>
        /// After the tag.
//@        ^After the tag.
        private void OutsideTheTag()
        {
        }

        /// <summary>
        /// </summary>
        /// Between tags.
//@        ^Between tags.
        /// <param name="x">
        /// </param>
        private void BetweenTags(int x)
        {
        }
    }
}
