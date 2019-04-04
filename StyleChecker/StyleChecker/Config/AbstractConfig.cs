namespace StyleChecker.Config
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Base configuration class.
    /// </summary>
    public abstract class AbstractConfig
    {
        /// <summary>
        /// The default namespace of the configuration XML document.
        /// </summary>
        public const string Namespace
            = "https://maroontress.com/StyleChecker/config.v1";

        /// <summary>
        /// Gets the result of the validation.
        /// </summary>
        /// <returns>
        /// The tuples representing the location and error message.
        /// </returns>
        public abstract IEnumerable<(int line, int column, string message)>
            Validate();
    }
}
