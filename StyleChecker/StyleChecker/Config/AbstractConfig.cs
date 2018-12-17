namespace StyleChecker.Config
{
    using System.Collections.Generic;

    /// <summary>
    /// Base configuaration class.
    /// </summary>
    public abstract class AbstractConfig
    {
        /// <summary>
        /// Gets the result of the validation.
        /// </summary>
        /// <returns>
        /// The strings representing errors.
        /// </returns>
        public abstract IEnumerable<string> Validate();
    }
}
