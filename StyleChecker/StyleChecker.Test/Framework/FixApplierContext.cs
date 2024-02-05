namespace StyleChecker.Test.Framework;

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
    /// <summary>
    /// Initializes a new instance of the <see cref="FixApplierContext"/>
    /// class.
    /// </summary>
    /// <param name="analyzer">
    /// The diagnostics analyzer.
    /// </param>
    /// <param name="documents">
    /// The source documents.
    /// </param>
    public FixApplierContext(
        DiagnosticAnalyzer analyzer,
        IEnumerable<Document> documents)
    {
        SourceDocuments = documents.ToImmutableArray();
        CompilerDiagnostics = documents
            .SelectMany(d => Documents.GetCompilerDiagnostics(d))
            .ToImmutableArray();
        AnalyzerDiagnostics = Diagnostics.GetSorted(
                analyzer, SourceDocuments, Atmosphere.Default)
            .ToImmutableArray();
    }

    /// <summary>
    /// Gets the <c>Document</c>s.
    /// </summary>
    public ImmutableArray<Document> SourceDocuments { get; }

    /// <summary>
    /// Gets the <c>Diagnostic</c>s of the compiler.
    /// </summary>
    public ImmutableArray<Diagnostic> CompilerDiagnostics { get; }

    /// <summary>
    /// Gets the <c>Diagnostic</c>s of the analyzer.
    /// </summary>
    public ImmutableArray<Diagnostic> AnalyzerDiagnostics { get; }
}
