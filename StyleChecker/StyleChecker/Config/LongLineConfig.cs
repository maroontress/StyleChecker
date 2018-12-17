namespace StyleChecker.Config
{
    using System;
    using System.Collections.Generic;
    using Maroontress.Oxbind;

    /// <summary>
    /// The configuration data of LongLine analyzer.
    /// </summary>
    [ForElement("LongLine")]
    public sealed class LongLineConfig : AbstractConfig
    {
        private const int DefaultMaxLineLength = 80;

        [field: ForAttribute("maxLineLength")]
        private string MaxLineLengthValue { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            string[] ToArray(params string[] args) => args;

            if (MaxLineLengthValue == null)
            {
                return Array.Empty<string>();
            }
            var (isValid, value) = ParseInt(MaxLineLengthValue);
            if (!isValid)
            {
                return ToArray(
                    "invalid integer value of maxLineLength attribute.");
            }
            if (value <= 0)
            {
                return ToArray(
                    "non-positive integer value of maxLineLength attribute.");
            }
            return Array.Empty<string>();
        }

        /// <summary>
        /// Gets the maximum line length.
        /// </summary>
        /// <seealso cref="Size.LongLine.Analyzer"/>
        /// <returns>
        /// The maximum line length.
        /// </returns>
        public int GetMaxLineLength()
        {
            return (MaxLineLengthValue == null
                    || !int.TryParse(MaxLineLengthValue, out var value)
                    || value <= 0)
                ? DefaultMaxLineLength
                : value;
        }

        private static (bool, int) ParseInt(string s)
        {
            var b = int.TryParse(s, out var value);
            return (b, value);
        }
    }
}
