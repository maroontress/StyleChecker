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
        private const string BaseDir = "Naming/Underscore";

        private string ReadText(string name)
        {
            return File.ReadAllText($"{BaseDir}/{name}.cs");
        }

        [TestMethod]
        public void Empty()
        {
            VerifyCSharpDiagnostic(@"");
        }

        [TestMethod]
        public void Okay()
        {
            VerifyCSharpDiagnostic(ReadText("Okay"));
        }

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            Func<int, int, string, DiagnosticResult> expected
                = (col, row, token) => new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(
                        "The name '{0}' includes a underscore.",
                        token),
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", col, row)
                    }
                };

            VerifyCSharpDiagnostic(
                code,
                expected(9, 17, "_alpha"),
                expected(10, 17, "foo_bar"),
                expected(11, 17, "_foo_bar_baz_"),
                expected(14, 36, "_args"),
                expected(16, 39, "_s"),
                expected(20, 34, "_hello"),
                expected(21, 25, "_action"),
                expected(22, 19, "_n"),
                expected(23, 30, "_action2"),
                expected(24, 20, "_n"),
                expected(24, 24, "_m"),
                expected(25, 18, "_localFunc"),
                expected(25, 33, "_v"));
            VerifyCSharpFix(code, fix);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CodeFixer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new Analyzer();
        }
    }
}
