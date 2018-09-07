namespace TestHelper
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Superclass of all Unit tests made for diagnostics with codefixes.
    /// Contains methods used to verify correctness of codefixes
    /// </summary>
    public abstract partial class CodeFixVerifier : DiagnosticVerifier
    {
        /// <summary>
        /// Gets the codefix being tested (C#) - to be implemented in
        /// non-abstract class
        /// </summary>
        /// <returns>
        /// The CodeFixProvider to be used for CSharp code
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
        /// Called to test a C# codefix when applied on the inputted string as
        /// a source
        /// </summary>
        /// <param name="oldSource">
        /// A class in the form of a string before the CodeFix was applied to
        /// it
        /// </param>
        /// <param name="newSource">
        /// A class in the form of a string after the CodeFix was applied to it
        /// </param>
        /// <param name="codeFixIndex">
        /// Index determining which codefix to apply if there are multiple
        /// </param>
        /// <param name="allowNewCompilerDiagnostics">
        /// A bool controlling whether or not the test will fail if the CodeFix
        /// introduces other warnings after being applied
        /// </param>
        protected void VerifyCSharpFix(
            string oldSource,
            string newSource,
            int? codeFixIndex = null,
            bool allowNewCompilerDiagnostics = false)
        {
            VerifyFix(
                CSharpDiagnosticAnalyzer,
                CSharpCodeFixProvider,
                oldSource,
                newSource,
                codeFixIndex,
                allowNewCompilerDiagnostics);
        }

        /// <summary>
        /// General verifier for codefixes. Creates a Document from the source
        /// string, then gets diagnostics on it and applies the relevant
        /// codefixes. Then gets the string after the codefix is applied and
        /// compares it with the expected result. Note: If any codefix causes
        /// new diagnostics to show up, the test fails unless
        /// allowNewCompilerDiagnostics is set to true.
        /// </summary>
        /// <param name="analyzer">
        /// The analyzer to be applied to the source code
        /// </param>
        /// <param name="codeFixProvider">
        /// The codefix to be applied to the code wherever the relevant
        /// Diagnostic is found
        /// </param>
        /// <param name="oldSource">
        /// A class in the form of a string before the CodeFix was applied to
        /// it
        /// </param>
        /// <param name="newSource">
        /// A class in the form of a string after the CodeFix was applied to it
        /// </param>
        /// <param name="codeFixIndex">
        /// Index determining which codefix to apply if there are multiple
        /// </param>
        /// <param name="allowNewCompilerDiagnostics">
        /// A bool controlling whether or not the test will fail if the CodeFix
        /// introduces other warnings after being applied
        /// </param>
        private void VerifyFix(
            DiagnosticAnalyzer analyzer,
            CodeFixProvider codeFixProvider,
            string oldSource,
            string newSource,
            int? codeFixIndex,
            bool allowNewCompilerDiagnostics)
        {
            var document = CreateDocument(oldSource);
            var analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(
                    analyzer, Singleton(document), Environment.Default);
            var compilerDiagnostics = GetCompilerDiagnostics(document);
            var attempts = analyzerDiagnostics.Length;

            for (var i = 0; i < attempts; ++i)
            {
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(
                    document,
                    analyzerDiagnostics[0],
                    (a, d) => actions.Add(a),
                    CancellationToken.None);
                codeFixProvider.RegisterCodeFixesAsync(context).Wait();

                if (!actions.Any())
                {
                    break;
                }

                if (codeFixIndex != null)
                {
                    document = ApplyFix(
                        document,
                        actions.ElementAt((int)codeFixIndex));
                    break;
                }

                document = ApplyFix(document, actions.ElementAt(0));
                var text = GetStringFromDocument(document);
                analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(
                    analyzer, Singleton(document), Environment.Default);

                var newCompilerDiagnostics = GetNewDiagnostics(
                    compilerDiagnostics, GetCompilerDiagnostics(document));

                // check if applying the code fix introduced any new compiler
                // diagnostics
                if (!allowNewCompilerDiagnostics
                    && newCompilerDiagnostics.Any())
                {
                    // Format and get the compiler diagnostics again so that
                    // the locations make sense in the output
                    document = document.WithSyntaxRoot(
                        Formatter.Format(
                            document.GetSyntaxRootAsync().Result,
                            Formatter.Annotation,
                            document.Project.Solution.Workspace));
                    newCompilerDiagnostics = GetNewDiagnostics(
                        compilerDiagnostics,
                        GetCompilerDiagnostics(document));

                    Assert.IsTrue(
                        false,
                        string.Format(
                            "Fix introduced new compiler diagnostics:\r\n"
                                + "{0}\r\n"
                                + "\r\n"
                                + "New document:\r\n"
                                + "{1}\r\n",
                            string.Join(
                                "\r\n",
                                newCompilerDiagnostics
                                    .Select(d => d.ToString())),
                            document.GetSyntaxRootAsync()
                                .Result
                                .ToFullString()));
                }

                // check if there are analyzer diagnostics left after the code
                // fix
                if (!analyzerDiagnostics.Any())
                {
                    break;
                }
            }

            // after applying all of the code fixes, compare the resulting
            // string to the inputted one
            var actual = GetStringFromDocument(document);
            var actualArray = actual.Split("\r\n");
            var expectedArray = newSource.Split("\r\n");
            var lines = actualArray.Length;
            for (var k = 0; k < lines; ++k)
            {
                if (!expectedArray[k].Equals(actualArray[k]))
                {
                    Assert.Fail(
                        $"line {k + 1}: "
                        + $"expected={expectedArray[k]}, "
                        + $"actual={actualArray[k]}");
                }
            }
            Assert.AreEqual(newSource, actual);
        }
    }
}
