namespace TestHelper
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;

    [Serializable]
    internal class CompilationException : Exception
    {
        private ImmutableArray<Diagnostic> rawDiagnostics;

        public CompilationException(
            string message,
            ImmutableArray<Diagnostic> rawDiagnostics)
                : base(message)
            => this.rawDiagnostics = rawDiagnostics;
    }
}
