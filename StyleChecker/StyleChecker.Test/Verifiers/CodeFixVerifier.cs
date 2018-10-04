namespace TestHelper
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Superclass of all Unit tests made for diagnostics with CodeFixes.
    /// Contains methods used to verify correctness of CodeFixes.
    /// </summary>
    public abstract partial class CodeFixVerifier : DiagnosticVerifier
    {
        /// <summary>
        /// Gets the CodeFix being tested (C#) - to be implemented in
        /// non-abstract class.
        /// </summary>
        /// <returns>
        /// The CodeFixProvider to be used for CSharp code.
        /// </returns>
        protected virtual CodeFixProvider CSharpCodeFixProvider => null;

        /// <summary>
        /// Returns a new array of <c>DiagnosticResultLocation</c> containing
        /// the single element representing the specified line and column.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="column">The column.</param>
        /// <returns>
        /// A new array of <c>DiagnosticResultLocation</c> containing
        /// the single element representing the specified line and column.
        /// </returns>
        protected static DiagnosticResultLocation[] SingleLocation(
            int line, int column)
        {
            return Singleton(
                new DiagnosticResultLocation("Test0.cs", line, column));
        }

        /// <summary>
        /// Creates a new <c>CodeChange</c> from entire texts representing for
        /// the specified file and the fixed file.
        /// </summary>
        /// <param name="name">
        /// The name of the source file to be read on the base directory. The
        /// extension ".cs" is not needed. For example, if the name is
        /// <c>"Code"</c>, the two files <c>Code.cs</c> and <c>CodeFix.cs</c>
        /// are to be read.
        /// </param>
        /// <returns>
        /// The <c>CodeChange</c> object.
        /// </returns>
        protected CodeChange ReadCodeChange(string name)
        {
            var code = ReadText(name);
            var codeFix = ReadText($"{name}Fix");
            return new CodeChange(code, codeFix);
        }

        /// <summary>
        /// Called to test a C# CodeFix when applied on the inputted string as
        /// a source.
        /// </summary>
        /// <param name="oldSource">
        /// A class in the form of a string before the CodeFix was applied to
        /// it.
        /// </param>
        /// <param name="newSource">
        /// A class in the form of a string after the CodeFix was applied to
        /// it.
        /// </param>
        /// <param name="allowNewCompilerDiagnostics">
        /// A bool controlling whether or not the test will fail if the CodeFix
        /// introduces other warnings after being applied.
        /// </param>
        protected void VerifyCSharpFix(
            string oldSource,
            string newSource,
            bool allowNewCompilerDiagnostics = false)
        {
            var codeChanges = Singleton(new CodeChange(oldSource, newSource));
            VerifyFix(codeChanges, allowNewCompilerDiagnostics);
        }

        /// <summary>
        /// Verifies the result of <c>CodeFix</c>es. Creates a <c>Document</c>s
        /// from the <c>CodeChange</c>s, then gets diagnostics on it and
        /// applies the relevant <c>CodeFix</c>es. Then gets the string after
        /// the <c>CodeFix</c> is applied and compares it with the expected
        /// result. Note: If any <c>CodeFix</c> causes new diagnostics to show
        /// up, the test fails unless
        /// <paramref name="allowNewCompilerDiagnostics"/> is set to true.
        /// </summary>
        /// <param name="codeChanges">
        /// The sources in the form of a string before/after the <c>CodeFix</c>
        /// was applied to it.
        /// </param>
        /// <param name="allowNewCompilerDiagnostics">
        /// A bool controlling whether or not the test will fail if the
        /// <c>CodeFix</c> introduces other warnings after being applied.
        /// </param>
        protected void VerifyFix(
            IEnumerable<CodeChange> codeChanges,
            bool allowNewCompilerDiagnostics = false)
        {
            var analyzer = CSharpDiagnosticAnalyzer;
            var codeFixProvider = CSharpCodeFixProvider;
            var verifier = new Verifier(analyzer, codeFixProvider);

            var codeChangeArray = codeChanges.ToArray();
            var expectedMap = new Dictionary<DocumentId, string>();
            var project = CreateProject(
                codeChanges,
                (id, c) => expectedMap[id] = c.After);
            var documents = project.Documents.ToArray();
            var firstVerifyContext = verifier.AnalyzeDocuments(documents);
            var maxTryCount = firstVerifyContext.AnalyzerDiagnostics.Length;
            Assert.IsTrue(maxTryCount > 0);

            var verifyContext = firstVerifyContext;
            var newDocumentMap = verifier.ModifyDocuments(verifyContext);
            for (var k = 0; k < maxTryCount; ++k)
            {
                var newDocuments = newDocumentMap.Values.ToArray();
                var newVerifyContext = verifier.AnalyzeDocuments(newDocuments);
                var newCompilerDiagnostics = newVerifyContext.CompilerDiagnostics;
                var diagnosticsDelta = GetNewDiagnostics(
                    firstVerifyContext.CompilerDiagnostics,
                    newCompilerDiagnostics);

                // Checks if applying the code fix introduced any new compiler
                // diagnostics.
                if (!allowNewCompilerDiagnostics && diagnosticsDelta.Any())
                {
                    FailFixIntroducedNewCompilerDiagnostics(
                        newDocuments, newCompilerDiagnostics);
                }

                verifyContext = newVerifyContext;
                if (!verifyContext.AnalyzerDiagnostics.Any())
                {
                    break;
                }
                newDocumentMap = verifier.ModifyDocuments(verifyContext);
            }
            Assert.IsTrue(!verifyContext.AnalyzerDiagnostics.Any());

            foreach (var id in project.DocumentIds)
            {
                var actual = GetStringFromDocument(newDocumentMap[id]);
                var expected = expectedMap[id];
                Compare(id, actual, expected);
            }
        }

        private void FailFixIntroducedNewCompilerDiagnostics(
            Document[] newDocuments, Diagnostic[] compilerDiagnostics)
        {
            // Format and get the compiler diagnostics again so that
            // the locations make sense in the output.
            var formatteDocuments = newDocuments
                .Select(d => d.WithSyntaxRoot(Formatter.Format(
                    d.GetSyntaxRootAsync().Result,
                    Formatter.Annotation,
                    d.Project.Solution.Workspace)))
                .ToArray();
            var newCompilerDiagnostics = formatteDocuments
                .SelectMany(d => Verifier.GetCompilerDiagnostics(d))
                .ToArray();
            var diagnosticsDelta = GetNewDiagnostics(
                compilerDiagnostics, newCompilerDiagnostics);

            var diagnosticMessages = string.Join(
                "\r\n",
                diagnosticsDelta.Select(d => d.ToString()));
            var soucres = formatteDocuments
                .Select(d => d.GetSyntaxRootAsync().Result.ToFullString());
            Assert.Fail(
                "Fix introduced new compiler diagnostics:\r\n"
                + $"{diagnosticMessages}");
        }

        /// <summary>
        /// Compares the specified actual source and the specified expected
        /// source and then if there is any difference in them, asserts to fail
        /// the test case with showing the specified <c>DocumentId</c> and the
        /// location of the first difference.
        /// </summary>
        /// <param name="id">
        /// The <c>DocumentId</c> of the source to compare.
        /// </param>
        /// <param name="actual">
        /// The actual source that the <c>CodeFix</c> provider provides.
        /// </param>
        /// <param name="expected">
        /// The expected source that the <c>CodeFix</c> provider is supposed to
        /// provide.
        /// </param>
        private void Compare(DocumentId id, string actual, string expected)
        {
            var actualArray = actual.Split("\r\n");
            var expectedArray = expected.Split("\r\n");
            var lines = actualArray.Length;
            for (var k = 0; k < lines; ++k)
            {
                if (!expectedArray[k].Equals(actualArray[k]))
                {
                    Assert.Fail(
                        $"id {id}: line {k + 1}: "
                        + $"expected={expectedArray[k]}, "
                        + $"actual={actualArray[k]}");
                }
            }
            Assert.AreEqual(expected, actual);
        }
    }
}
