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
        {
            T[] ToArray<T>(params T[] args) => args;

            (int, int, string) ToError(BindEvent<string> ev, string message)
                => (ev.Line, ev.Column, $"{message}: '{ev.Value}'");

            if (MaxLineLengthEvent is null)
            {
                return NoError;
            }
            var (isValid, value) = ParseInt(MaxLineLengthEvent.Value);
            if (!isValid)
            {
                return ToArray(ToError(
                    MaxLineLengthEvent,
                    "invalid integer value of maxLineLength attribute"));
            }
            if (value <= 0)
            {
                return ToArray(ToError(
                    MaxLineLengthEvent,
                    "non-positive integer value of maxLineLength attribute"));
            }
            return NoError;
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
            return (MaxLineLengthEvent is null
                    || !int.TryParse(MaxLineLengthEvent.Value, out var value)
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
