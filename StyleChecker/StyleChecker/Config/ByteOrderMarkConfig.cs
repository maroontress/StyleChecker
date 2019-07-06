#pragma warning disable CA1812, CA1823

namespace StyleChecker.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using Maroontress.Oxbind;
    using StyleChecker.Cleaning.ByteOrderMark;

    /// <summary>
    /// The configuration data of ByteOrderMark analyzer.
    /// </summary>
    [ForElement(Analyzer.DiagnosticId, Namespace)]
    public sealed class ByteOrderMarkConfig : AbstractConfig
    {
        [ElementSchema]
        private static readonly Schema TheSchema = Schema.Of(
            Multiple.Of<File>());

        [field: ForAttribute("maxDepth")]
        private BindEvent<string>? MaxDepthEvent { get; }

        [field: ForChild]
        private IEnumerable<File> Files { get; } = Enumerable.Empty<File>();

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
                .OfType<string>();

        /// <inheritdoc/>
        public override IEnumerable<(int, int, string)> Validate()
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
}
