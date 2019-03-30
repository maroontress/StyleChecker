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

        [field: ForChild]
        private IEnumerable<File> Files { get; }
            = Array.Empty<File>();

        /// <summary>
        /// Gets all the glob patterns.
        /// </summary>
        /// <returns>
        /// All the glob patterns.
        /// </returns>
        public IEnumerable<string> GetGlobs() => Files.Select(e => e.Glob);

        /// <inheritdoc/>
        public override IEnumerable<(int, int, string)> Validate()
            => NoError;

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
