namespace StyleChecker.Test.Framework
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents compilation errors.
    /// </summary>
    [Serializable]
    public class CompilationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CompilationException"/> class.
        /// </summary>
        /// <param name="message">
        /// The error message.
        /// </param>
        /// <param name="rawDiagnostics">
        /// The diagnostics of the compiler.
        /// </param>
        public CompilationException(
            string message,
            ImmutableArray<Diagnostic> rawDiagnostics)
                : base(message)
            => RawDiagnostics = rawDiagnostics;

        public CompilationException()
        {
        }

        public CompilationException(string message)
            : base(message)
        {
        }

        public CompilationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ImmutableArray<Diagnostic> RawDiagnostics { get; }
    }
}
