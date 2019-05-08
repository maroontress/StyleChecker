namespace StyleChecker.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Maroontress.Oxbind;

    /// <summary>
    /// Provides utilities for parsing values.
    /// </summary>
    public static class ParseKit
    {
        /// <summary>
        /// Represents that there is no error.
        /// </summary>
        public static readonly
            IEnumerable<(int line, int column, string message)> NoError
                = Enumerable.Empty<(int, int, string)>();

        /// <summary>
        /// Gets the integer value of the specified BindEvent&lt;string&gt;
        /// object.
        /// </summary>
        /// <param name="ev">
        /// The BindEvent&lt;string&gt; object that provides an integer value.
        /// </param>
        /// <param name="defaultValue">
        /// The default value.
        /// </param>
        /// <param name="isValidValue">
        /// The function that returns whether a value of the argument is valid
        /// or not.
        /// </param>
        /// <returns>
        /// The integer value if the specified BindEvent has a value and the
        /// value is parsed successfully and valid, the default value
        /// otherwise.
        /// </returns>
        public static int ToIntValue(
            BindEvent<string> ev,
            int defaultValue,
            Func<int, bool> isValidValue)
        {
            if (ev is null)
            {
                return defaultValue;
            }
            var v = ParseInt(ev.Value);
            return (v.HasValue && isValidValue(v.Value))
                ? v.Value
                : defaultValue;
        }

        /// <summary>
        /// Validates the specified BindEvent&lt;string&gt; object and gets the
        /// tuples representing the error information.
        /// </summary>
        /// <param name="ev">
        /// The BindEvent&lt;string&gt; object.
        /// </param>
        /// <param name="isValidValue">
        /// The function that returns whether a value of the argument is valid
        /// or not.
        /// </param>
        /// <param name="invalidIntegerValueError">
        /// The error message when it is unable to parse an integer value.
        /// </param>
        /// <param name="invalidValueRangeError">
        /// The error message when the parsed value is invalid.
        /// </param>
        /// <returns>
        /// <see cref="NoError"/> if the specified BindEvent&lt;string&gt;
        /// object can be parsed successfully. Otherwise, the tuple containing
        /// the line number, the column number and the error message.
        /// </returns>
        public static IEnumerable<(int, int, string)> ValidateInt(
            BindEvent<string> ev,
            Func<int, bool> isValidValue,
            string invalidIntegerValueError,
            string invalidValueRangeError)
        {
            if (ev is null)
            {
                return NoError;
            }
            var v = ParseInt(ev.Value);
            if (!v.HasValue)
            {
                return Enumerables.Of(ToError(
                    ev, invalidIntegerValueError));
            }
            if (!isValidValue(v.Value))
            {
                return Enumerables.Of(ToError(
                    ev, invalidValueRangeError));
            }
            return NoError;
        }

        private static (int, int, string) ToError(
            BindEvent<string> ev, string message)
            => (ev.Line, ev.Column, $"{message}: '{ev.Value}'");

        /// <summary>
        /// Gets the integer value that results from parsing the specified
        /// string.
        /// </summary>
        /// <param name="s">
        /// The string representing an integer value.
        /// </param>
        /// <returns>
        /// The tuple of the boolean value representing whether the value has
        /// been parsed successfully and the parsed integer value.
        /// </returns>
        private static int? ParseInt(string s)
        {
            return int.TryParse(s, out var value) ? value : (int?)null;
        }
    }
}
