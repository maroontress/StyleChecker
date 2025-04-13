namespace StyleChecker.Analyzers.Config;

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
    /// The errors.
    /// </returns>
    IEnumerable<WhereWhy> Validate();
}
