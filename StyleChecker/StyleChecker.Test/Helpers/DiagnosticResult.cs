namespace TestHelper
{
    using System;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Stores information about a <c>Diagnostic</c> appearing in a source.
    /// </summary>
    public struct DiagnosticResult
    {
        private DiagnosticResultLocation[] locations;

        /// <summary>
        /// Gets or sets the locations.
        /// </summary>
        public DiagnosticResultLocation[] Locations
        {
            get => locations ?? Array.Empty<DiagnosticResultLocation>();
            set => locations = value;
        }

        /// <summary>
        /// Gets or sets the severity.
        /// </summary>
        public DiagnosticSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the analyzer's code.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the description message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets the path of the first location.
        /// </summary>
        public string Path => Locations.Length > 0 ? Locations[0].Path : "";

        /// <summary>
        /// Gets the line number of the first location.
        /// </summary>
        public int Line => Locations.Length > 0 ? Locations[0].Line : -1;

        /// <summary>
        /// Gets the column number of the first location.
        /// </summary>
        public int Column => Locations.Length > 0 ? Locations[0].Column : -1;
    }
}
