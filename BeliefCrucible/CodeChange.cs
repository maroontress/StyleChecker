namespace BeliefCrucible;

/// <summary>
/// Represents two codes.
/// </summary>
/// <param name="Before">
/// The code before change.
/// </param>
/// <param name="After">
/// The code after change.
/// </param>
public record CodeChange(string Before, string After)
{
    /// <summary>
    /// Creates a CodeChange instance based on the specified name and function.
    /// </summary>
    /// <param name="name">
    /// The name of the code file.
    /// </param>
    /// <param name="readText">
    /// A function to read the text of the code file.
    /// </param>
    /// <returns>
    /// A new CodeChange instance.
    /// </returns>
    public static CodeChange Of(string name, Func<string, string> readText)
    {
        var code = readText(name);
        var codeFix = readText($"{name}_Fixed");
        return new(code, codeFix);
    }
}
