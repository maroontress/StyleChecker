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
    /// Class for turning strings into documents and getting the diagnostics on
    /// them. All methods are static.
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
        /// Given source codes in the form of strings, and a
        /// <c>DiagnosticAnalyzer</c> to apply to it, return the diagnostics
        /// found in the string after converting it to a document.
        /// </summary>
        /// <param name="sources">
        /// Classes in the form of strings.
        /// </param>
        /// <param name="analyzer">
        /// The analyzer to be run on the sources.
        /// </param>
        /// <param name="environment">
        /// The environment.
        /// </param>
        /// <returns>
        /// An array of <c>Diagnostic</c>s that surfaced in the source code,
        /// sorted by <c>Location</c>.
        /// </returns>
        private static Diagnostic[] GetSortedDiagnostics(
            string[] sources,
            DiagnosticAnalyzer analyzer,
            Environment environment)
        {
            return GetSortedDiagnosticsFromDocuments(
                analyzer, GetDocuments(sources), environment);
        }

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
        /// <param name="environment">
        /// The environment.
        /// </param>
        /// <returns>
        /// An array of <c>Diagnostic</c>s that surfaced in the source code,
        /// sorted by <c>Location</c>.
        /// </returns>
        public static Diagnostic[] GetSortedDiagnosticsFromDocuments(
            DiagnosticAnalyzer analyzer,
            Document[] documents,
            Environment environment)
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
                    "System.Runtime.dll",
                })
                .Select(dll => Path.Combine(assemblyPath, dll))
                .Select(path => MetadataReference.CreateFromFile(path))
                .ToImmutableArray();
            var excludeIdSet = environment.ExcludeIds.ToImmutableHashSet();

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
                var configText = environment.ConfigText;
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

            var diagnosticList = documents
                .Select(d => d.Project)
                .SelectMany(p => DiagnosticArrayOf(p))
                .Where(d => ValidLocation(d.Location))
                .ToList();

            return SortDiagnostics(diagnosticList);
        }

        /// <summary>
        /// Sorts diagnostics by location in source document.
        /// </summary>
        /// <param name="diagnostics">
        /// The list of Diagnostics to be sorted.
        /// </param>
        /// <returns>
        /// An array containing the Diagnostics in order of Location.
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
        /// a project and return the documents.
        /// </summary>
        /// <param name="sources">
        /// Classes in the form of strings.
        /// </param>
        /// <returns>
        /// The <c>Document</c>s produced from the sources.
        /// </returns>
        private static Document[] GetDocuments(string[] sources)
        {
            var documents = CreateProject(sources).Documents.ToArray();

            if (sources.Length != documents.Length)
            {
                throw new InvalidOperationException(
                    "Amount of sources did not match amount of Documents "
                    + "created");
            }

            return documents;
        }

        /// <summary>
        /// Creates a Document from a string through creating a project that
        /// contains it.
        /// </summary>
        /// <param name="source">
        /// Classes in the form of a string.
        /// </param>
        /// <returns>
        /// A Document created from the source string.
        /// </returns>
        protected static Document CreateDocument(string source)
        {
            return CreateProject(Singleton(source)).Documents.First();
        }

        /// <summary>
        /// Creates a project using the specified strings as sources.
        /// </summary>
        /// <param name="sources">
        /// Classes in the form of strings.
        /// </param>
        /// <returns>
        /// A Project created out of the Documents created from the source
        /// strings.
        /// </returns>
        private static Project CreateProject(IEnumerable<string> sources)
        {
            return CreateProject(sources, s => s, (id, s) => { });
        }

        /// <summary>
        /// Creates a project using the specified <c>CodeChange</c>s as
        /// sources.
        /// </summary>
        /// <param name="codeChanges">
        /// The <c>CodeChange</c>s representing sources.
        /// </param>
        /// <param name="notifyDocumentId">
        /// The function that consumes a <c>DocumentId</c> object and the
        /// <c>CodeChange</c> object.
        /// </param>
        /// <returns>
        /// The new project.
        /// </returns>
        protected static Project CreateProject(
            IEnumerable<CodeChange> codeChanges,
            Action<DocumentId, CodeChange> notifyDocumentId)
        {
            return CreateProject(
                codeChanges, c => c.Before, notifyDocumentId);
        }

        /// <summary>
        /// Creates a project using the specified code suppliers.
        /// </summary>
        /// <typeparam name="T">
        /// The type that supplies a source.
        /// </typeparam>
        /// <param name="codeSuppliers">
        /// The suppliers to supply sources.
        /// </param>
        /// <param name="toString">
        /// The function that consumes a code supplier and then returns a
        /// source in the form of a <c>string</c>.
        /// </param>
        /// <param name="notifyDocumentId">
        /// The function that consumes a <c>DocumentId</c> object and the code
        /// supplier.
        /// </param>
        /// <returns>
        /// The new project.
        /// </returns>
        private static Project CreateProject<T>(
            IEnumerable<T> codeSuppliers,
            Func<T, string> toString,
            Action<DocumentId, T> notifyDocumentId)
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

            var codeSupplierArray = codeSuppliers.ToArray();
            var n = codeSupplierArray.Length;
            for (var k = 0; k < n; ++k)
            {
                var codeSupplier = codeSupplierArray[k];
                var source = toString(codeSupplier);
                var newFileName = fileNamePrefix + k + "." + fileExt;
                var documentId = DocumentId.CreateNewId(
                    projectId, debugName: newFileName);
                solution = solution.AddDocument(
                    documentId, newFileName, SourceText.From(source));
                notifyDocumentId(documentId, codeSupplier);
            }
            return solution.GetProject(projectId);
        }

        #endregion
    }
}
