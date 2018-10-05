namespace StyleChecker.Test.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Provides the utility methods for <c>Project</c>s.
    /// </summary>
    public static class Projects
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
        public static Project Of(IEnumerable<string> sources)
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
        public static Project Of(
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
    }
}
