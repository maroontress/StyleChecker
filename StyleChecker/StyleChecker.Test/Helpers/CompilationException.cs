namespace TestHelper
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents compilation errors.
    /// </summary>
    [Serializable]
    internal class CompilationException : Exception
    {
        private ImmutableArray<Diagnostic> rawDiagnostics;

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
            => this.rawDiagnostics = rawDiagnostics;
    }
}
