#pragma warning disable CA1812, CA1823

namespace StyleChecker.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
        /// Gets the paths of the file that must not start with a BOM.
        /// </summary>
        /// <returns>
        /// The paths of the file that must not start with a BOM.
        /// </returns>
        public IEnumerable<string> GetPaths()
        {
            var slash = new[] { '/' };

            string ToPath(string slashPath)
                => Path.Combine(
                    slashPath.Split(
                        slash, StringSplitOptions.RemoveEmptyEntries));

            return Files.Select(e => ToPath(e.Path));
        }

        /// <inheritdoc/>
        public override IEnumerable<(int, int, string)> Validate()
            => NoError;

        /// <summary>
        /// Represents the files that must not start with a BOM.
        /// </summary>
        [ForElement("file", Namespace)]
        private sealed class File
        {
            /// <summary>
            /// Gets the path of the file that is disallowed to start with a
            /// BOM.
            /// </summary>
            [field: ForAttribute("path")]
            public string Path { get; }
        }
    }
}
