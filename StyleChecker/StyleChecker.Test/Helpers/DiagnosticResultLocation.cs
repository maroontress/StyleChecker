namespace TestHelper
{
    using System;

    /// <summary>
    /// Location where the diagnostic appears, as determined by path, line
    /// number, and column number.
    /// </summary>
    public struct DiagnosticResultLocation
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="DiagnosticResultLocation"/> struct.
        /// </summary>
        /// <param name="path">The path of the source file.</param>
        /// <param name="line">The line number.</param>
        /// <param name="column">The column number.</param>
        public DiagnosticResultLocation(string path, int line, int column)
        {
            if (line < -1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(line), "line must be >= -1");
            }

            if (column < -1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(column), "column must be >= -1");
            }

            Path = path;
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Gets the path of the source file.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the column number.
        /// </summary>
        public int Column { get; }
    }
}
