namespace StyleChecker.Config
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides methods for data validation.
    /// </summary>
    public interface Validateable
    {
        /// <summary>
        /// Gets the result of the validation.
        /// </summary>
        /// <returns>
        /// The tuples representing the location and error message.
        /// </returns>
        IEnumerable<(int line, int column, string message)> Validate();
    }
}
