namespace StyleChecker;

/// <summary>
/// Provides the help link URI.
/// </summary>
public static class HelpLink
{
    /// <summary>
    /// Gets the help link URI of the specified analyzer's ID.
    /// </summary>
    /// <param name="id">
    /// The analyzer's ID.
    /// </param>
    /// <returns>
    /// The help link URI.
    /// </returns>
    public static string ToUri(string id)
    {
        return "https://github.com/maroontress/StyleChecker"
            + $"/blob/master/doc/rules/{id}.md";
    }
}
