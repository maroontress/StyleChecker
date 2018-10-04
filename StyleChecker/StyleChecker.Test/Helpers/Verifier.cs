namespace TestHelper
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public sealed class Verifier
    {
        public Verifier(
            DiagnosticAnalyzer analyzer,
            CodeFixProvider codeFixProvider)
        {
            Analyzer = analyzer;
            CodeFixProvider = codeFixProvider;
        }

        public DiagnosticAnalyzer Analyzer { get; private set; }

        public CodeFixProvider CodeFixProvider { get; private set; }

        /// <summary>
        /// Gets the existing compiler diagnostics on the specified document.
        /// </summary>
        /// <param name="document">
        /// The <c>Document</c> to run the compiler diagnostic analyzers on.
        /// </param>
        /// <returns>
        /// The compiler diagnostics that were found in the code.
        /// </returns>
        public static IEnumerable<Diagnostic> GetCompilerDiagnostics(
            Document document)
        {
            return document.GetSemanticModelAsync().Result.GetDiagnostics();
        }

        public VerifierContext AnalyzeDocuments(Document[] documents)
        {
            var compilerDiagnostics = documents
                .SelectMany(d => GetCompilerDiagnostics(d))
                .ToArray();
            var analyzerDiagnostics
                = DiagnosticVerifier.GetSortedDiagnosticsFromDocuments(
                    Analyzer, documents, Environment.Default);
            return new VerifierContext()
            {
                AnalyzerDiagnostics = analyzerDiagnostics,
                CompilerDiagnostics = compilerDiagnostics,
                Documents = documents,
            };
        }

        public Dictionary<DocumentId, Document>
            ModifyDocuments(VerifierContext verifyContext)
        {
            var documents = verifyContext.Documents;
            var analyzerDiagnostics = verifyContext.AnalyzerDiagnostics;
            var actions = new List<CodeAction>();
            var context = new CodeFixContext(
                documents[0],
                analyzerDiagnostics[0],
                (a, ignored) => actions.Add(a),
                CancellationToken.None);
            CodeFixProvider.RegisterCodeFixesAsync(context).Wait();
            Assert.IsTrue(actions.Any());
            var operations = actions[0]
                    .GetOperationsAsync(CancellationToken.None)
                    .Result;
            var solution = operations
                .OfType<ApplyChangesOperation>()
                .Single()
                .ChangedSolution;
            return documents.Select(d => solution.GetDocument(d.Id))
                .ToDictionary(d => d.Id);
        }
    }
}
