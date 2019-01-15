namespace StyleChecker.Config
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base configuaration class.
    /// </summary>
    public abstract class AbstractConfig
    {
        /// <summary>
        /// The default namespace of the configualation XML document.
        /// </summary>
        public const string Namespace
            = "https://maroontress.com/StyleChecker/config.v1";

        /// <summary>
        /// Represents that there is no error.
        /// </summary>
        protected static readonly
            IEnumerable<(int line, int column, string message)> NoError
                = Array.Empty<(int, int, string)>();

        /// <summary>
        /// Gets the result of the validation.
        /// </summary>
        /// <returns>
        /// The tuples represeting the location and error message.
        /// </returns>
        public abstract IEnumerable<(int line, int column, string message)>
            Validate();
    }
}
