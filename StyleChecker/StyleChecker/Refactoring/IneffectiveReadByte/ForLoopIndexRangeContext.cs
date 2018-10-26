namespace StyleChecker.Refactoring.IneffectiveReadByte
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// The context used in analyzing a <c>for</c> statement.
    /// </summary>
    public sealed class ForLoopIndexRangeContext
    {
        private SyntaxToken id0;
        private int startValue;
        private SyntaxToken id1;
        private SyntaxToken lessThanOrLessThanEqual;
        private int endValue;
        private SyntaxToken id2;

        /// <summary>
        /// Gets the identifier of the loop index.
        /// </summary>
        public SyntaxToken Id => id0;

        /// <summary>
        /// Gets the start index of the range.
        /// </summary>
        public int Start => startValue;

        /// <summary>
        /// Gets the end index of the range.
        /// </summary>
        public int End => endValue;

        /// <summary>
        /// Notifies the identifier and constant value when the initializer is
        /// the form <c>id = value;</c>.
        /// </summary>
        /// <param name="id">
        /// The identifier of the initializer.
        /// </param>
        /// <param name="value">
        /// The constant value.
        /// </param>
        public void First(SyntaxToken id, SyntaxToken value)
        {
            id0 = id;
            startValue = int.Parse(value.Text);
        }

        /// <summary>
        /// Notifies the identifier and constant value when the expression is
        /// the form <c>leftId &lt; rightValue;</c>
        /// or <c>leftId &lt;= rightValue;</c>.
        /// </summary>
        /// <param name="leftId">
        /// The identifier that is the left operand of the expression.
        /// </param>
        /// <param name="operatorToken">
        /// The operator that is LessThan or LessThanEqual.
        /// </param>
        /// <param name="rightValue">
        /// The constant value that is the right operand of the expression.
        /// </param>
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

        /// <summary>
        /// Notifies the identifier when the incrementor is the form
        /// <c>++id</c> or <c>id++</c>.
        /// </summary>
        /// <param name="id">
        /// The identifier to increment.
        /// </param>
        public void Third(SyntaxToken id)
        {
            id2 = id;
        }

        /// <summary>
        /// Gets whether this context makes sense.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the <c>for</c> loop has an incremental index and its
        /// range is constant.
        /// </returns>
        public bool IsValid()
        {
            return id0.Text.Equals(id1.Text)
                && id0.Text.Equals(id2.Text)
                && startValue <= endValue;
        }
    }
}
