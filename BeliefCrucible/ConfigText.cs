namespace BeliefCrucible;

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using StyleChecker.Annotations;

/// <summary>
/// Represents content of the configuration file.
/// </summary>
/// <param name="text">
/// The text representing content of the configuration file.
/// </param>
public sealed class ConfigText(string text) : AdditionalText
{
    /// <inheritdoc/>
    public override string Path => "StyleChecker.xml";

    private string Text { get; } = text;

    /// <summary>
    /// Returns a new <c>AnalyzerOptions</c> including the additional file
    /// whose content represents the specified text.
    /// </summary>
    /// <param name="text">
    /// The text representing content of the configuration file.
    /// </param>
    /// <returns>
    /// The new <c>AnalyzerOptions</c>.
    /// </returns>
    public static AnalyzerOptions ToAnalyzerOptions(string text)
        => new([new ConfigText(text)]);

    /// <inheritdoc/>
    public override SourceText GetText(
        [Unused] CancellationToken cancellationToken = default)
    {
        return SourceText.From(Text);
    }
}
