namespace StyleChecker.Test.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Simplification;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Superclass of all Unit tests made for diagnostics with CodeFixes.
    /// Contains methods used to verify correctness of CodeFixes.
    /// </summary>
    public abstract class CodeFixVerifier : DiagnosticVerifier
    {
        private static readonly string NewLine = Environment.NewLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFixVerifier"/>
        /// class.
        /// </summary>
        /// <param name="baseDir">
        /// The base directory to read files.
        /// </param>
        /// <param name="analyer">
        /// The diagnostic analyzer being tested.
        /// </param>
        /// <param name="codeFixProvider">
        /// The CodeFix provider being tested.
        /// </param>
        protected CodeFixVerifier(
            string baseDir,
            DiagnosticAnalyzer analyer,
            CodeFixProvider codeFixProvider)
            : base(baseDir, analyer)
        {
            CodeFixProvider = codeFixProvider;
        }

        /// <summary>
        /// Gets the CodeFix provider being tested.
        /// </summary>
        /// <returns>
        /// The CodeFix provider being tested.
        /// </returns>
        protected CodeFixProvider CodeFixProvider { get; }

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
        /// Tests the analyzer and CodeFix provider. Verifies each of
        /// diagnostics found in the specified source, compared with the result
        /// the specified function extracts from the beliefs embedded from the
        /// source. And then, applies the CodeFix to the decoded source,
        /// comparing the result with the specified expected source.
        /// </summary>
        /// <param name="encodedSource">
        /// The encoded source where the beliefs have been embedded.
        /// </param>
        /// <param name="atmosphere">
        /// The compilation environment.
        /// </param>
        /// <param name="toResult">
        /// The function that returns the expected diagnostic result with the
        /// specified belief.
        /// </param>
        /// <param name="codeFix">
        /// A expected source to be equal to the decoded source to which the
        /// CodeFix is applied.
        /// </param>
        protected void VerifyDiagnosticAndFix(
            string encodedSource,
            Atmosphere atmosphere,
            Func<Belief, Result> toResult,
            string codeFix)
        {
            var (source, expected) = Beliefs.Decode(
                encodedSource, atmosphere, toResult);
            VerifyDiagnostics(
                Arrays.Create(source),
                DiagnosticAnalyzer,
                atmosphere,
                expected);
            VerifyFix(source, codeFix);
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
        /// A <c>bool</c> controlling whether or not the test will fail if the
        /// CodeFix introduces other warnings after being applied.
        /// </param>
        protected void VerifyFix(
            string oldSource,
            string newSource,
            bool allowNewCompilerDiagnostics = false)
        {
            var codeChanges = Arrays.Create(
                new CodeChange(oldSource, newSource));
            VerifyFix(codeChanges, allowNewCompilerDiagnostics);
        }

        /// <summary>
        /// Verifies the result of CodeFixes. Creates a <c>Document</c>s
        /// from the <c>CodeChange</c>s, then gets diagnostics on it and
        /// applies the relevant CodeFixes. Then gets the string after
        /// the CodeFix is applied and compares it with the expected
        /// result. Note: If any CodeFix causes new diagnostics to show
        /// up, the test fails unless
        /// <paramref name="allowNewCompilerDiagnostics"/> is set to true.
        /// </summary>
        /// <param name="codeChanges">
        /// The sources in the form of a string before/after the CodeFix
        /// was applied to it.
        /// </param>
        /// <param name="allowNewCompilerDiagnostics">
        /// A <c>bool</c> controlling whether or not the test will fail if the
        /// CodeFix introduces other warnings after being applied.
        /// </param>
        protected void VerifyFix(
            IEnumerable<CodeChange> codeChanges,
            bool allowNewCompilerDiagnostics = false)
        {
            var analyzer = DiagnosticAnalyzer;
            var codeFixProvider = CodeFixProvider;
            var applier = new FixApplier(analyzer, codeFixProvider);

            var expectedMap = new Dictionary<DocumentId, string>();
            var project = Projects.Of(
                codeChanges,
                (id, c) => expectedMap[id] = c.After);
            var documents = project.Documents.ToArray();
            var firstApplierContext = applier.Analyze(documents);
            var maxTryCount = firstApplierContext.AnalyzerDiagnostics.Length;
            Assert.IsTrue(maxTryCount > 0);

            var verifyContext = firstApplierContext;
            var newDocumentMap = applier.Modify(verifyContext);
            for (var k = 0; k < maxTryCount; ++k)
            {
                var newDocuments = newDocumentMap.Values.ToArray();
                var newApplierContext = applier.Analyze(newDocuments);
                var newCompilerDiagnostics
                    = newApplierContext.CompilerDiagnostics;
                var diagnosticsDelta = Diagnostics.GetNewDelta(
                    firstApplierContext.CompilerDiagnostics,
                    newCompilerDiagnostics);

                // Checks if applying the code fix introduced any new compiler
                // diagnostics.
                if (!allowNewCompilerDiagnostics && diagnosticsDelta.Any())
                {
                    FailFixIntroducedNewCompilerDiagnostics(
                        newDocuments, newCompilerDiagnostics);
                }

                verifyContext = newApplierContext;
                if (!verifyContext.AnalyzerDiagnostics.Any())
                {
                    break;
                }
                newDocumentMap = applier.Modify(verifyContext);
            }
            Assert.IsTrue(!verifyContext.AnalyzerDiagnostics.Any());

            foreach (var id in project.DocumentIds)
            {
                var actual = ToString(newDocumentMap[id]);
                var expected = expectedMap[id];
                Compare(id, actual, expected);
            }
        }

        private static void FailFixIntroducedNewCompilerDiagnostics(
            IEnumerable<Document> newDocuments,
            IEnumerable<Diagnostic> compilerDiagnostics)
        {
            // Format and get the compiler diagnostics again so that
            // the locations make sense in the output.
            var formattedDocuments = newDocuments
                .Select(d => d.WithSyntaxRoot(Formatter.Format(
                    d.GetSyntaxRootAsync().Result,
                    Formatter.Annotation,
                    d.Project.Solution.Workspace)));
            var newCompilerDiagnostics = formattedDocuments
                .SelectMany(d => Documents.GetCompilerDiagnostics(d));
            var diagnosticsDelta = Diagnostics.GetNewDelta(
                compilerDiagnostics, newCompilerDiagnostics);

            var diagnosticMessages = string.Join(
                NewLine,
                diagnosticsDelta.Select(d => d.ToString()));
            var sources = string.Join(
                $"{NewLine}{NewLine}",
                formattedDocuments.Select(
                    d => d.GetSyntaxRootAsync().Result.ToFullString()));
            Assert.Fail(
                $"Fix introduced new compiler diagnostics:{NewLine}"
                + $"{diagnosticMessages}{NewLine}"
                + $"{NewLine}"
                + $"New documents:{NewLine}"
                + $"{sources}{NewLine}");
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
        /// The actual source that the CodeFix provider provides.
        /// </param>
        /// <param name="expected">
        /// The expected source that the CodeFix provider is supposed to
        /// provide.
        /// </param>
        private static void Compare(
            DocumentId id, string actual, string expected)
        {
            var actualArray = actual.Split(NewLine);
            var expectedArray = expected.Split(NewLine);
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

        /// <summary>
        /// Returns the string representing the specified document
        /// based on the syntax root.
        /// </summary>
        /// <param name="document">
        /// The <c>>Document</c> to be converted to a string.
        /// </param>
        /// <returns>
        /// A string containing the syntax of the Document after formatting.
        /// </returns>
        private static string ToString(Document document)
        {
            var simplifiedDoc = Simplifier.ReduceAsync(
                document, Simplifier.Annotation).Result;
            var root = simplifiedDoc.GetSyntaxRootAsync().Result;
            root = Formatter.Format(
                root,
                Formatter.Annotation,
                simplifiedDoc.Project.Solution.Workspace);
            return root.GetText().ToString();
        }
    }
}
