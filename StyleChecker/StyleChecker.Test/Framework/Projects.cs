namespace StyleChecker.Test.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;
    using Enumerables = Maroontress.Util.Enumerables;

    /// <summary>
    /// Provides the utility methods for <c>Project</c>s.
    /// </summary>
    public static class Projects
    {
        private const string DefaultFilePathPrefix = "Test";
        private const string CSharpDefaultFileExt = "cs";
        private const string TestProjectName = "TestProject";

        /// <summary>
        /// Gets all <c>MetadataReference</c>s.
        /// </summary>
        public static IEnumerable<MetadataReference>
            AllReferences { get; } = Enumerables.Of(
                NewDllReference("System.Runtime"),
                NewDllReference("netstandard"),
                /* System.Private.CoreLib */
                NewReference<object>(),
                /* System.Linq */
                NewReference(typeof(Enumerable)),
                /* System.Console */
                NewReference(typeof(Console)),
                /* System.Collections.Immutable */
                NewReference(typeof(ImmutableArray)),
                /* Microsoft.CodeAnalysis.CSharp */
                NewReference<CSharpCompilation>(),
                /* Microsoft.CodeAnalysis */
                NewReference<Compilation>(),
                /* System.Runtime.Extensions */
                NewReference<StringReader>());

        /// <summary>
        /// Creates a project using the specified strings as sources.
        /// </summary>
        /// <param name="atmosphere">
        /// The compliation environment.
        /// </param>
        /// <param name="sources">
        /// Classes in the form of strings.
        /// </param>
        /// <returns>
        /// A Project created out of the Documents created from the source
        /// strings.
        /// </returns>
        public static Project Of(
            Atmosphere atmosphere, params string[] sources)
        {
            return CreateProject(
                atmosphere, sources, s => s, (id, s) => { });
        }

        /// <summary>
        /// Gets a new <c>MetadataReference</c> associated with the assembly
        /// containing the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type that the assembly contains.
        /// </typeparam>
        /// <returns>
        /// The new <c>MetadataReference</c>.
        /// </returns>
        public static MetadataReference NewReference<T>()
            => NewReference(typeof(T));

        /// <summary>
        /// Gets a new <c>MetadataReference</c> associated with the assembly
        /// containing the specified type.
        /// </summary>
        /// <param name="type">
        /// The type that the assembly contains.
        /// </param>
        /// <returns>
        /// The new <c>MetadataReference</c>.
        /// </returns>
        public static MetadataReference NewReference(Type type)
            => MetadataReference.CreateFromFile(type.Assembly.Location);

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
                Atmosphere.Default,
                codeChanges,
                c => c.Before,
                notifyDocumentId);
        }

        /// <summary>
        /// Creates a project using the specified code suppliers.
        /// </summary>
        /// <typeparam name="T">
        /// The type that supplies a source.
        /// </typeparam>
        /// <param name="atmosphere">
        /// The compilation environment.
        /// </param>
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
            Atmosphere atmosphere,
            IEnumerable<T> codeSuppliers,
            Func<T, string> toString,
            Action<DocumentId, T> notifyDocumentId)
        {
            var basePath = atmosphere.BasePath;
            var projectId = ProjectId.CreateNewId(TestProjectName);
            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(
                    projectId,
                    TestProjectName,
                    TestProjectName,
                    LanguageNames.CSharp)
                .AddMetadataReferences(projectId, AllReferences);

            var codeSupplierArray = codeSuppliers.ToArray();
            var n = codeSupplierArray.Length;
            for (var k = 0; k < n; ++k)
            {
                var codeSupplier = codeSupplierArray[k];
                var source = toString(codeSupplier);
                var newFileName
                    = $"{DefaultFilePathPrefix}{k}.{CSharpDefaultFileExt}";
                var path = basePath is null
                    ? newFileName
                    : Path.Combine(basePath, newFileName);
                var documentId = DocumentId.CreateNewId(
                    projectId, debugName: path);
                solution = solution.AddDocument(
                    documentId, path, SourceText.From(source));
                notifyDocumentId(documentId, codeSupplier);
            }

            var project = solution.GetProject(projectId);
            var parseOption = project.ParseOptions
                .WithDocumentationMode(atmosphere.DocumentationMode);
            return project.WithParseOptions(parseOption);
        }

        private static MetadataReference NewDllReference(string name)
        {
            var dllPath = typeof(object).Assembly.Location;
            var basePath = Path.GetDirectoryName(dllPath)
                ?? throw new DirectoryNotFoundException(dllPath);

            return MetadataReference.CreateFromFile(
                Path.Combine(basePath, $"{name}.dll"));
        }
    }
}
