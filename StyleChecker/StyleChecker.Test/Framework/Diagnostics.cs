namespace StyleChecker.Test.Framework
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleChecker.Annotations;

    /// <summary>
    /// Provides the utility methods for <c>Diagnosic</c>s.
    /// </summary>
    public static class Diagnostics
    {
        /// <summary>
        /// Given an analyzer and a document to apply it to, run the analyzer
        /// and gather an array of diagnostics found in it. The returned
        /// diagnostics are then ordered by location in the source document.
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
        /// An array of <c>Diagnostic</c>s that surfaced in the source code,
        /// sorted by <c>Location</c>.
        /// </returns>
        public static IEnumerable<Diagnostic> GetSorted(
            DiagnosticAnalyzer analyzer,
            IEnumerable<Document> documents,
            Atmosphere atmosphere)
        {
            var analyzerArray = ImmutableArray.Create(analyzer);
            var treeSet = documents
                .Select(d => d.GetSyntaxTreeAsync().Result)
                .ToHashSet();
            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary);
            string BasePath(System.Type type)
                => Path.GetDirectoryName(type.Assembly.Location);
            var assemblyPath = BasePath(typeof(object));
            var references = new[]
                {
                    "System.Private.CoreLib.dll",
                    "System.Console.dll",
                    "System.Runtime.dll",
                    "System.Collections.Immutable.dll",
                }
                .Select(dll => Path.Combine(assemblyPath, dll))
                .Concat(new[]
                {
                    Path.Combine(
                        BasePath(typeof(UnusedAttribute)),
                        "StyleChecker.dll"),
                })
                .Select(path => MetadataReference.CreateFromFile(path))
                .ToImmutableArray();
            var excludeIdSet = atmosphere.ExcludeIds.ToImmutableHashSet();

            ImmutableArray<Diagnostic> DiagnosticArrayOf(Project p)
            {
                var compilation = p.WithCompilationOptions(options)
                    .AddMetadataReferences(references)
                    .GetCompilationAsync()
                    .Result;
                var rawDiagnostics = compilation.GetDiagnostics()
                    .Where(d => !excludeIdSet.Contains(d.Id))
                    .ToImmutableArray();
                if (rawDiagnostics.Length > 0)
                {
                    var m = string.Join(',', rawDiagnostics);
                    throw new CompilationException(m, rawDiagnostics);
                }
                var configText = atmosphere.ConfigText;
                var analyzerOptions = (configText == null)
                    ? null
                    : ConfigText.ToAnalyzerOptions(configText);
                return compilation
                    .WithAnalyzers(analyzerArray, analyzerOptions)
                    .GetAnalyzerDiagnosticsAsync()
                    .Result;
            }

            bool ValidLocation(Location location)
            {
                return location == Location.None
                    || location.IsInMetadata
                    || treeSet.Contains(location.SourceTree);
            }

            return documents.Select(d => d.Project)
                .SelectMany(p => DiagnosticArrayOf(p))
                .Where(d => ValidLocation(d.Location))
                .OrderBy(d => d.Location.SourceSpan.Start);
        }

        /// <summary>
        /// Compares two collections of <c>Diagnostic</c>s, and returns a list
        /// of any new diagnostics that appear only in the second collection.
        /// Note: Considers <c>Diagnostic</c>s to be the same if they have the
        /// same IDs. In the case of multiple diagnostics with the same ID in a
        /// row, this method may not necessarily return the new one.
        /// </summary>
        /// <param name="diagnostics">
        /// The <c>Diagnostic</c>s that existed in the code before the CodeFix
        /// was applied.
        /// </param>
        /// <param name="newDiagnostics">
        /// The <c>Diagnostic</c>s that exist in the code after the CodeFix was
        /// applied.
        /// </param>
        /// <returns>
        /// A list of <c>Diagnostic</c>s that only surfaced in the code after
        /// the CodeFix was applied.
        /// </returns>
        public static IEnumerable<Diagnostic> GetNewDelta(
            IEnumerable<Diagnostic> diagnostics,
            IEnumerable<Diagnostic> newDiagnostics)
        {
            var oldArray = diagnostics
                .OrderBy(d => d.Location.SourceSpan.Start)
                .ToArray();
            var newArray = newDiagnostics
                .OrderBy(d => d.Location.SourceSpan.Start)
                .ToArray();

            var oldIndex = 0;
            var newIndex = 0;

            while (newIndex < newArray.Length)
            {
                if (oldIndex < oldArray.Length
                    && oldArray[oldIndex].Id == newArray[newIndex].Id)
                {
                    ++oldIndex;
                    ++newIndex;
                }
                else
                {
                    yield return newArray[newIndex++];
                }
            }
        }
    }
}
