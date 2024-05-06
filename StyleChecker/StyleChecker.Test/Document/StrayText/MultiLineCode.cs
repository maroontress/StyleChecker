namespace StyleChecker.Test.Document.StrayText
{
    public sealed class MultiLineCode
    {
        /** Not in the tag. */
//@        ^Not in the tag.
        private void NotInTheTag()
        {
        }

        /**
         * Not in the tag.
         */
//@       ^^Not in the tag.
        private void NotInTheTagWithLeadingAsterisks()
        {
        }

        /**
         * Before the tag.
         * <summary>
         * </summary>
         * After the tag.
         */
//@       ^^^^^Before the tag.
//@       ^^After the tag.
        private void OutsideTheTag()
        {
        }

        /**
         * <summary>
         * </summary>
         * Between tags.
         * <param name="x">
         * </param>
         */
//@       ^^^^Between tags.
        private void BetweenTags(int x)
        {
        }

    }
}
