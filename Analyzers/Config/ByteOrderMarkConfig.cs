namespace StyleChecker.Analyzers.Config;

using System.Collections.Generic;
using System.Linq;
using Maroontress.Oxbind;
using Maroontress.Roastery;
using StyleChecker.Analyzers.Cleaning.ByteOrderMark;

/// <summary>
/// The configuration data of ByteOrderMark analyzer.
/// </summary>
[ForElement(Analyzer.DiagnosticId, Namespace)]
public sealed class ByteOrderMarkConfig(
    [ForAttribute("maxDepth")] BindResult<string>? maxDepthEvent,
    [Multiple] IEnumerable<ByteOrderMarkConfig.File> files)
    : AbstractConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ByteOrderMarkConfig"/>
    /// class.
    /// </summary>
    [Ignored]
    public ByteOrderMarkConfig()
        : this(null, [])
    {
    }

    private BindResult<string>? MaxDepthEvent { get; } = maxDepthEvent;

    private IEnumerable<File> Files { get; } = files;

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
    public sealed class File([ForAttribute("glob")] string? glob)
    {
        /// <summary>
        /// Gets the glob pattern representing files that are disallowed to
        /// start with a BOM.
        /// </summary>
        public string? Glob { get; } = glob;
    }
}
