namespace Analyzers.Config;

using System.Collections.Generic;
using System.Linq;
using Maroontress.Oxbind;
using Maroontress.Roastery;

/// <summary>
/// The configuration data of ThoughtlessName analyzer.
/// </summary>
[ForElement("ThoughtlessName", Namespace)]
public sealed class ThoughtlessNameConfig : AbstractConfig
{
#pragma warning disable IDE0052 // Remove unread private members
    [ElementSchema]
    private static readonly Schema TheSchema = Schema.Of(
        Multiple.Of<Disallow>());
#pragma warning restore IDE0052 // Remove unread private members

    [field: ForChild]
    private IEnumerable<Disallow> DisallowElements { get; } = [];

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
    private sealed class Disallow
    {
        /// <summary>
        /// Gets the identifier to be disallowed.
        /// </summary>
        [field: ForAttribute("id")]
        public string? Id { get; }
    }
}
