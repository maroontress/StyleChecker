namespace Analyzers.Config;

using System.Collections.Generic;
using System.Linq;
using Analyzers.Cleaning.ByteOrderMark;
using Maroontress.Oxbind;
using Maroontress.Roastery;

/// <summary>
/// The configuration data of ByteOrderMark analyzer.
/// </summary>
[ForElement(Analyzer.DiagnosticId, Namespace)]
public sealed class ByteOrderMarkConfig : AbstractConfig
{
#pragma warning disable IDE0052 // Remove unread private members
    [ElementSchema]
    private static readonly Schema TheSchema = Schema.Of(Multiple.Of<File>());
#pragma warning restore IDE0052 // Remove unread private members

    [field: ForAttribute("maxDepth")]
    private BindEvent<string>? MaxDepthEvent { get; }

    [field: ForChild]
    private IEnumerable<File> Files { get; } = [];

    /// <summary>
    /// Gets the maximum number of directory levels to search.
    /// </summary>
    /// <returns>
    /// The maximum number of directory levels to search.
    /// </returns>
    public int GetMaxDepth() => ParseKit.ToIntValue(
        MaxDepthEvent,
        PathFinder.DefaultMaxDepth,
        v => v > 0);

    /// <summary>
    /// Gets all the glob patterns.
    /// </summary>
    /// <returns>
    /// All the glob patterns.
    /// </returns>
    public IEnumerable<string> GetGlobs()
        => Files.Select(e => e.Glob)
            .FilterNonNullReference();

    /// <inheritdoc/>
    public override IEnumerable<WhereWhy> Validate()
        => ParseKit.ValidateInt(
            MaxDepthEvent,
            v => v > 0,
            "invalid integer value of maxDepth attribute",
            "non-positive integer value of maxDepth attribute");

    /// <summary>
    /// Represents the files that must not start with a BOM.
    /// </summary>
    [ForElement("files", Namespace)]
    private sealed class File
    {
        /// <summary>
        /// Gets the glob pattern representing files that are disallowed to
        /// start with a BOM.
        /// </summary>
        [field: ForAttribute("glob")]
        public string? Glob { get; }
    }
}
