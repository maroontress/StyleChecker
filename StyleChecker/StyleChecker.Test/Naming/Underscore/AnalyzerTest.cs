namespace StyleChecker.Test.Naming.Underscore
{
    using System;
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Naming.Underscore;
    using TestHelper;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override CodeFixProvider CSharpCodeFixProvider
            => new CodeFixer();

        protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Naming", "Underscore");

        [TestMethod]
        public void Empty()
        {
            VerifyCSharpDiagnostic(@"", EmptyIds);
        }

        [TestMethod]
        public void Okay()
        {
            VerifyCSharpDiagnostic(ReadText("Okay"), EmptyIds);
        }

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            var startOffset = 12;
            Func<int, int, string, DiagnosticResult> expected
                = (row, col, token) => new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(
                        "The name '{0}' includes a underscore.",
                        token),
                    Severity = DiagnosticSeverity.Warning,
                    Locations = SingleLocation(startOffset + row, col)
                };

            VerifyCSharpDiagnostic(
                code,
                EmptyIds,
                expected(0, 17, "_alpha"),
                expected(1, 17, "foo_bar"),
                expected(2, 17, "_foo_bar_baz_"),
                expected(5, 36, "_args"),
                expected(7, 39, "_s"),
                expected(11, 34, "_hello"),
                expected(12, 25, "_action"),
                expected(13, 19, "_n"),
                expected(14, 30, "_action2"),
                expected(15, 20, "_n"),
                expected(15, 24, "_m"),
                expected(16, 18, "_localFunc"),
                expected(16, 33, "_v"));
            VerifyCSharpFix(code, fix);
        }
    }
}
