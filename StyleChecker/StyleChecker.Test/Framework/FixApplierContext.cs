namespace StyleChecker.Test.Framework
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// The context of <see cref="FixApplier"/>.
    /// </summary>
    public sealed class FixApplierContext
    {
        public FixApplierContext(
            DiagnosticAnalyzer analyzer,
            IEnumerable<Document> documents)
        {
            SourceDocuments = documents.ToImmutableArray();
            CompilerDiagnostics = documents
                .SelectMany(d => Documents.GetCompilerDiagnostics(d))
                .ToImmutableArray();
            AnalyzerDiagnostics = Diagnostics.GetSorted(
                    analyzer, SourceDocuments, Environment.Default)
                .ToImmutableArray();
        }

        /// <summary>
        /// Gets the <c>Document</c>s.
        /// </summary>
        public ImmutableArray<Document> SourceDocuments
            { get; private set; }

        /// <summary>
        /// Gets the <c>Diagnostic</c>s of the compiler.
        /// </summary>
        public ImmutableArray<Diagnostic> CompilerDiagnostics
            { get; private set; }

        /// <summary>
        /// Gets the <c>Diagnostic</c>s of the analyzer.
        /// </summary>
        public ImmutableArray<Diagnostic> AnalyzerDiagnostics
            { get; private set; }
    }
}
