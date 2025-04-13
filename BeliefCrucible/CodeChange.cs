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
}
