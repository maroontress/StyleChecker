#pragma warning disable CA1812, CA1823

namespace StyleChecker.Config
{
    using System;
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
        private BindEvent<string> MaxDepthEvent { get; }

        [field: ForChild]
        private IEnumerable<File> Files { get; } = Array.Empty<File>();

        /// <summary>
        /// Gets the maximum number of directory levels to search.
        /// </summary>
        /// <returns>
        /// The maximum number of directory levels to search.
        /// </returns>
        public int GetMaxDepth()
        {
            return (MaxDepthEvent is null
                    || !int.TryParse(MaxDepthEvent.Value, out var value)
                    || value <= 0)
                ? PathFinder.DefaultMaxDepth
                : value;
        }

        /// <summary>
        /// Gets all the glob patterns.
        /// </summary>
        /// <returns>
        /// All the glob patterns.
        /// </returns>
        public IEnumerable<string> GetGlobs() => Files.Select(e => e.Glob);

        /// <inheritdoc/>
        public override IEnumerable<(int, int, string)> Validate()
        {
            (int, int, string) ToError(BindEvent<string> ev, string message)
                => (ev.Line, ev.Column, $"{message}: '{ev.Value}'");

            if (MaxDepthEvent is null)
            {
                return NoError;
            }
            var (isValid, value) = ParseInt(MaxDepthEvent.Value);
            if (!isValid)
            {
                return Enumerables.Of(ToError(
                    MaxDepthEvent,
                    "invalid integer value of maxDepth attribute"));
            }
            if (value <= 0)
            {
                return Enumerables.Of(ToError(
                    MaxDepthEvent,
                    "non-positive integer value of maxDepth attribute"));
            }
            return NoError;
        }

        private static (bool, int) ParseInt(string s)
        {
            var b = int.TryParse(s, out var value);
            return (b, value);
        }

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
            public string Glob { get; }
        }
    }
}
