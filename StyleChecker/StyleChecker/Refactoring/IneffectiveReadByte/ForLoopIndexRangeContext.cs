namespace StyleChecker.Refactoring.IneffectiveReadByte
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public sealed class ForLoopIndexRangeContext
    {
        private SyntaxToken id0;
        private int startValue;
        private SyntaxToken id1;
        private SyntaxToken lessThanOrLessThanEqual;
        private int endValue;
        private SyntaxToken id2;

        public SyntaxToken Id => id0;

        public int Start => startValue;

        public int End => endValue;

        public void First(SyntaxToken id, SyntaxToken value)
        {
            id0 = id;
            startValue = int.Parse(value.Text);
        }

        public void Second(
            SyntaxToken leftId,
            SyntaxToken operatorToken,
            SyntaxToken rightValue)
        {
            id1 = leftId;
            lessThanOrLessThanEqual = operatorToken;
            endValue = int.Parse(rightValue.Text);
            if (lessThanOrLessThanEqual.IsKind(SyntaxKind.LessThanToken))
            {
                --endValue;
            }
        }

        public void Third(SyntaxToken id)
        {
            id2 = id;
        }

        public bool IsValid()
        {
            return id0.Text.Equals(id1.Text)
                && id0.Text.Equals(id2.Text)
                && startValue <= endValue;
        }
    }
}

