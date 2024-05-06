namespace StyleChecker.Test.Framework;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// The context of <see cref="FixApplier"/>.
/// </summary>
/// <param name="SourceDocuments">
/// The <c>Document</c>s.
/// </param>
/// <param name="CompilerDiagnostics">
/// The <c>Diagnostic</c>s of the compiler.
/// </param>
/// <param name="AnalyzerDiagnostics">
/// The <c>Diagnostic</c>s of the analyzer.
/// </param>
public record class FixApplierContext(
    ImmutableArray<Document> SourceDocuments,
    ImmutableArray<Diagnostic> CompilerDiagnostics,
    ImmutableArray<Diagnostic> AnalyzerDiagnostics)
{
    /// <summary>
    /// Creates a new instance of <see cref="FixApplierContext"/>.
    /// </summary>
    /// <param name="analyzer">
    /// The diagnostics analyzer.
    /// </param>
    /// <param name="documents">
    /// The source documents.
    /// </param>
    /// <returns>
    /// A new instance of <see cref="FixApplierContext"/>.
    /// </returns>
    public static FixApplierContext Of(
        DiagnosticAnalyzer analyzer, IEnumerable<Document> documents)
    {
        return new(
            documents.ToImmutableArray(),
            documents.SelectMany(Documents.GetCompilerDiagnostics)
                .ToImmutableArray(),
            Diagnostics.GetSorted(analyzer, documents, Atmosphere.Default)
                .ToImmutableArray());
    }
}
