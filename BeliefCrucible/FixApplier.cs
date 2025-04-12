namespace BeliefCrucible;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roastery;

/// <summary>
/// Provides a way to fix documents step by step with the diagnostic analyzer
/// and the CodeFix provider.
/// </summary>
/// <param name="Analyzer">
/// The diagnostic analyzer.
/// </param>
/// <param name="CodeFixProvider">
/// The CodeFix provider.
/// </param>
public record class FixApplier(
    DiagnosticAnalyzer Analyzer, CodeFixProvider CodeFixProvider)
{
    /// <summary>
    /// Analyzes the specified <c>Document</c>s and returns the context.
    /// </summary>
    /// <param name="documents">
    /// The <c>Document</c>s to be analyzed.
    /// </param>
    /// <returns>
    /// The context.
    /// </returns>
    public FixApplierContext Analyze(IEnumerable<Document> documents)
    {
        return FixApplierContext.Of(Analyzer, documents);
    }

    /// <summary>
    /// Modifies the <c>Document</c>s that the specified context represents,
    /// using the CodeFix provider.
    /// </summary>
    /// <param name="applierContext">
    /// The context.
    /// </param>
    /// <returns>
    /// A dictionary containing the new <c>Document</c>s modified with the
    /// CodeFix provider.
    /// </returns>
    public IReadOnlyDictionary<DocumentId, Document> Modify(
        FixApplierContext applierContext)
    {
        var documents = applierContext.SourceDocuments;
        var analyzerDiagnostics = applierContext.AnalyzerDiagnostics;
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(
            documents[0],
            analyzerDiagnostics[0],
            (a, ignored) => actions.Add(a),
            CancellationToken.None);
        CodeFixProvider.RegisterCodeFixesAsync(context)
            .Wait();
        if (actions.Count is 0)
        {
            throw new InvalidOperationException("No actions created.");
        }
        var operations = actions[0].GetOperationsAsync(CancellationToken.None)
            .Result;
        var solution = operations.OfType<ApplyChangesOperation>()
            .Single()
            .ChangedSolution;
        return documents.Select(d => solution.GetDocument(d.Id))
            .FilterNonNullReference()
            .ToFrozenDictionary(d => d.Id);
    }
}
