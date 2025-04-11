namespace BeliefCrucible;

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using StyleChecker.Annotations;
using Enumerables = CodeDebt.Util.Enumerables;

/// <summary>
/// Provides the utility methods for <c>Diagnosic</c>s.
/// </summary>
public static class Diagnostics
{
    /// <summary>
    /// Given an analyzer and a document to apply it to, run the analyzer and
    /// gather an array of diagnostics found in it. The returned diagnostics
    /// are then ordered by location in the source document.
    /// </summary>
    /// <param name="analyzer">
    /// The analyzer to run on the documents.
    /// </param>
    /// <param name="documents">
    /// The Documents that the analyzer will be run on.
    /// </param>
    /// <param name="atmosphere">
    /// The environment.
    /// </param>
    /// <returns>
    /// An array of <c>Diagnostic</c>s that surfaced in the source code, sorted
    /// by <c>Location</c>.
    /// </returns>
    public static IEnumerable<Diagnostic> GetSorted(
        DiagnosticAnalyzer analyzer,
        IEnumerable<Document> documents,
        Atmosphere atmosphere)
    {
        var analyzerArray = ImmutableArray.Create(analyzer);
        var treeSet = documents.Select(d => d.GetSyntaxTreeAsync().Result)
            .ToFrozenSet();
        var options = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary);
        var references = Enumerables.Of(
            Projects.NewReference<UnusedAttribute>());
        var excludeIdSet = atmosphere.ExcludeIds
            .ToFrozenSet();

        ImmutableArray<Diagnostic> DiagnosticArrayOf(Project p)
        {
            if (p.WithCompilationOptions(options)
                .AddMetadataReferences(references)
                .GetCompilationAsync()
                .Result is not {} compilation)
            {
                throw new CompilationException(
                    $"{p.Language}: not supported");
            }
            var rawDiagnostics = compilation.GetDiagnostics()
                .Where(d => !excludeIdSet.Contains(d.Id))
                .ToList();
            if (rawDiagnostics.Count is not 0)
            {
                throw new CompilationException(
                    "Compilation error", rawDiagnostics);
            }
            var analyzerOptions = atmosphere.ConfigText is {} configText
                ? ConfigText.ToAnalyzerOptions(configText)
                : null;
            return compilation.WithAnalyzers(analyzerArray, analyzerOptions)
                .GetAnalyzerDiagnosticsAsync()
                .Result;
        }

        bool ValidLocation(Location location)
        {
            return atmosphere.ForceLocationValid
                || location == Location.None
                || location.IsInMetadata
                || treeSet.Contains(location.SourceTree);
        }

        return documents.Select(d => d.Project)
            .SelectMany(p => DiagnosticArrayOf(p))
            .Where(d => ValidLocation(d.Location))
            .OrderBy(d => d.Location.SourceSpan.Start);
    }

    /// <summary>
    /// Compares two collections of <c>Diagnostic</c>s, and returns a list of
    /// any new diagnostics that appear only in the second collection. Note:
    /// Considers <c>Diagnostic</c>s to be the same if they have the same IDs.
    /// In the case of multiple diagnostics with the same ID in a row, this
    /// method may not necessarily return the new one.
    /// </summary>
    /// <param name="diagnostics">
    /// The <c>Diagnostic</c>s that existed in the code before the CodeFix was
    /// applied.
    /// </param>
    /// <param name="newDiagnostics">
    /// The <c>Diagnostic</c>s that exist in the code after the CodeFix was
    /// applied.
    /// </param>
    /// <returns>
    /// A list of <c>Diagnostic</c>s that only surfaced in the code after the
    /// CodeFix was applied.
    /// </returns>
    public static IEnumerable<Diagnostic> GetNewDelta(
        IEnumerable<Diagnostic> diagnostics,
        IEnumerable<Diagnostic> newDiagnostics)
    {
        static int ToStart(Diagnostic d)
            => d.Location.SourceSpan.Start;

        var oldList = diagnostics.OrderBy(ToStart)
            .ToList();
        var newList = newDiagnostics.OrderBy(ToStart)
            .ToList();
        var oldListSize = oldList.Count;
        var newListSize = newList.Count;
        var oldIndex = 0;
        var newIndex = 0;

        while (newIndex < newListSize)
        {
            if (oldIndex < oldListSize
                && oldList[oldIndex].Id == newList[newIndex].Id)
            {
                ++oldIndex;
                ++newIndex;
                continue;
            }
            yield return newList[newIndex];
            ++newIndex;
        }
    }
}
