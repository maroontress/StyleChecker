#pragma warning disable SA1124
#pragma warning disable SA1202

namespace TestHelper
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Class for turning strings into documents and getting the diagnostics on them
    /// All methods are static
    /// </summary>
    public abstract partial class DiagnosticVerifier
    {
        private static readonly MetadataReference CorlibReference
            = MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location);

        private static readonly MetadataReference SystemCoreReference
            = MetadataReference.CreateFromFile(
                typeof(Enumerable).Assembly.Location);

        private static readonly MetadataReference CSharpSymbolsReference
            = MetadataReference.CreateFromFile(
                typeof(CSharpCompilation).Assembly.Location);

        private static readonly MetadataReference CodeAnalysisReference
            = MetadataReference.CreateFromFile(
                typeof(Compilation).Assembly.Location);

        private static readonly string DefaultFilePathPrefix = "Test";
        private static readonly string CSharpDefaultFileExt = "cs";
        private static readonly string TestProjectName = "TestProject";

        #region  Get Diagnostics

        /// <summary>
        /// Given classes in the form of strings, their language, and an
        /// IDiagnosticAnalyzer to apply to it, return the diagnostics found in
        /// the string after converting it to a document.
        /// </summary>
        /// <param name="sources">
        /// Classes in the form of strings
        /// </param>
        /// <param name="analyzer">
        /// The analyzer to be run on the sources
        /// </param>
        /// <param name="excludeIds">
        /// All IDs of diagnostics to be ignored.
        /// </param>
        /// <returns>
        /// An IEnumerable of Diagnostics that surfaced in the source code,
        /// sorted by Location
        /// </returns>
        private static Diagnostic[] GetSortedDiagnostics(
            string[] sources,
            DiagnosticAnalyzer analyzer,
            string[] excludeIds)
        {
            return GetSortedDiagnosticsFromDocuments(
                analyzer, GetDocuments(sources), excludeIds);
        }

        /// <summary>
        /// Given an analyzer and a document to apply it to, run the analyzer
        /// and gather an array of diagnostics found in it. The returned
        /// diagnostics are then ordered by location in the source document.
        /// </summary>
        /// <param name="analyzer">
        /// The analyzer to run on the documents
        /// </param>
        /// <param name="documents">
        /// The Documents that the analyzer will be run on
        /// </param>
        /// <param name="excludeIds">
        /// All IDs of diagnostics to be ignored.
        /// </param>
        /// <returns>
        /// An IEnumerable of Diagnostics that surfaced in the source code,
        /// sorted by Location
        /// </returns>
        protected static Diagnostic[] GetSortedDiagnosticsFromDocuments(
            DiagnosticAnalyzer analyzer,
            Document[] documents,
            string[] excludeIds)
        {
            var analyzerArray = ImmutableArray.Create(analyzer);
            var treeSet = documents
                .Select(d => d.GetSyntaxTreeAsync().Result)
                .ToHashSet();
            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary);
            var assemblyPath = Path.GetDirectoryName(
                typeof(object).Assembly.Location);
            var references = (new string[]
                {
                    "System.Private.CoreLib.dll",
                    "System.Console.dll",
                    "System.Runtime.dll"
                })
                .Select(dll => Path.Combine(assemblyPath, dll))
                .Select(path => MetadataReference.CreateFromFile(path))
                .ToImmutableArray();
            var excludeIdSet = excludeIds.ToImmutableHashSet();

            ImmutableArray<Diagnostic> diagnosticArrayOf(Project p)
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
                return compilation.WithAnalyzers(analyzerArray)
                    .GetAnalyzerDiagnosticsAsync()
                    .Result;
            }

            bool validLocation(Location location)
            {
                return location == Location.None
                    || location.IsInMetadata
                    || treeSet.Contains(location.SourceTree);
            }

            var diagnosticList = documents
                .Select(d => d.Project)
                .SelectMany(p => diagnosticArrayOf(p))
                .Where(d => validLocation(d.Location))
                .ToList();

            return SortDiagnostics(diagnosticList);
        }

        /// <summary>
        /// Sort diagnostics by location in source document
        /// </summary>
        /// <param name="diagnostics">
        /// The list of Diagnostics to be sorted
        /// </param>
        /// <returns>
        /// An IEnumerable containing the Diagnostics in order of Location
        /// </returns>
        private static Diagnostic[] SortDiagnostics(
            IEnumerable<Diagnostic> diagnostics)
        {
            return diagnostics.OrderBy(d => d.Location.SourceSpan.Start)
                .ToArray();
        }

        #endregion

        #region Set up compilation and documents

        /// <summary>
        /// Given an array of strings as sources and a language, turn them into
        /// a project and return the documents and spans of it.
        /// </summary>
        /// <param name="sources">
        /// Classes in the form of strings
        /// </param>
        /// <returns>
        /// A Tuple containing the Documents produced from the sources and
        /// their TextSpans if relevant
        /// </returns>
        private static Document[] GetDocuments(string[] sources)
        {
            var project = CreateProject(sources);
            var documents = project.Documents.ToArray();

            if (sources.Length != documents.Length)
            {
                throw new InvalidOperationException(
                    "Amount of sources did not match amount of Documents "
                    + "created");
            }

            return documents;
        }

        /// <summary>
        /// Create a Document from a string through creating a project that
        /// contains it.
        /// </summary>
        /// <param name="source">Classes in the form of a string</param>
        /// <returns>A Document created from the source string</returns>
        protected static Document CreateDocument(string source)
        {
            return CreateProject(Singleton(source)).Documents.First();
        }

        /// <summary>
        /// Create a project using the inputted strings as sources.
        /// </summary>
        /// <param name="sources">
        /// Classes in the form of strings
        /// </param>
        /// <returns>
        /// A Project created out of the Documents created from the source
        /// strings
        /// </returns>
        private static Project CreateProject(string[] sources)
        {
            var language = LanguageNames.CSharp;
            var fileNamePrefix = DefaultFilePathPrefix;
            var fileExt = CSharpDefaultFileExt;

            var projectId = ProjectId.CreateNewId(debugName: TestProjectName);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(
                    projectId, TestProjectName, TestProjectName, language)
                .AddMetadataReference(projectId, CorlibReference)
                .AddMetadataReference(projectId, SystemCoreReference)
                .AddMetadataReference(projectId, CSharpSymbolsReference)
                .AddMetadataReference(projectId, CodeAnalysisReference);

            var count = 0;
            foreach (var source in sources)
            {
                var newFileName = fileNamePrefix + count + "." + fileExt;
                var documentId = DocumentId.CreateNewId(
                    projectId, debugName: newFileName);
                solution = solution.AddDocument(
                    documentId, newFileName, SourceText.From(source));
                ++count;
            }
            return solution.GetProject(projectId);
        }

        #endregion
    }
}
