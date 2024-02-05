#pragma warning disable CA1812, CA1823

namespace StyleChecker.Config;

using System.Collections.Generic;
using System.Linq;
using Maroontress.Extensions;
using Maroontress.Oxbind;

/// <summary>
/// The configuration data of ThoughtlessName analyzer.
/// </summary>
[ForElement("ThoughtlessName", Namespace)]
public sealed class ThoughtlessNameConfig : AbstractConfig
{
    [ElementSchema]
    private static readonly Schema TheSchema = Schema.Of(
        Multiple.Of<Disallow>());

    [field: ForChild]
    private IEnumerable<Disallow> DisallowElements { get; }
        = Enumerable.Empty<Disallow>();

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
