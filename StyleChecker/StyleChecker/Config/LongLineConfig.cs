namespace StyleChecker.Config
{
    using System.Collections.Generic;
    using Maroontress.Oxbind;

    /// <summary>
    /// The configuration data of LongLine analyzer.
    /// </summary>
    [ForElement("LongLine", Namespace)]
    public sealed class LongLineConfig : AbstractConfig
    {
        private const int DefaultMaxLineLength = 80;

        [field: ForAttribute("maxLineLength")]
        private BindEvent<string> MaxLineLengthEvent { get; }

        /// <inheritdoc/>
        public override IEnumerable<(int, int, string)> Validate()
            => ParseKit.Validate(
                MaxLineLengthEvent,
                v => v > 0,
                "invalid integer value of maxLineLength attribute",
                "non-positive integer value of maxLineLength attribute");

        /// <summary>
        /// Gets the maximum line length.
        /// </summary>
        /// <seealso cref="Size.LongLine.Analyzer"/>
        /// <returns>
        /// The maximum line length.
        /// </returns>
        public int GetMaxLineLength() => ParseKit.ToIntValue(
            MaxLineLengthEvent,
            DefaultMaxLineLength,
            v => v > 0);
    }
}
