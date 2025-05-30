namespace StyleChecker.Analyzers.Config;

using System.Collections.Generic;
using System.Linq;
using Maroontress.Oxbind;
using Maroontress.Roastery;

/// <summary>
/// The configuration data of ThoughtlessName analyzer.
/// </summary>
[ForElement("ThoughtlessName", Namespace)]
public sealed class ThoughtlessNameConfig(
    [Multiple] IEnumerable<ThoughtlessNameConfig.Disallow> disallowElements)
    : AbstractConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThoughtlessNameConfig"/>
    /// class.
    /// </summary>
    public ThoughtlessNameConfig()
        : this([])
    {
    }

    private IEnumerable<Disallow> DisallowElements { get; } = disallowElements;

    /// <summary>
    /// Gets the identifiers that must not be used.
    /// </summary>
    /// <returns>
    /// The identifiers that must not be used.
    /// </returns>
    public IEnumerable<string> GetDisallowedIdentifiers()
    {
        return DisallowElements.Select(e => e.Id)
            .FilterNonNullReference();
    }

    /// <inheritdoc/>
    public override IEnumerable<WhereWhy> Validate() => ParseKit.NoError;

    /// <summary>
    /// Represents the identifier that must not be used.
    /// </summary>
    [ForElement("disallow", Namespace)]
    public sealed class Disallow(
        [ForAttribute("id")] string? id)
    {
        /// <summary>
        /// Gets the identifier to be disallowed.
        /// </summary>
        public string? Id { get; } = id;
    }
}
