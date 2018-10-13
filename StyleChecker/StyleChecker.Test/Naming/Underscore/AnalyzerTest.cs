namespace StyleChecker.Test.Naming.Underscore
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Naming.Underscore;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override CodeFixProvider CodeFixProvider
            => new CodeFixer();

        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Naming", "Underscore");

        [TestMethod]
        public void Empty()
            => VerifyDiagnostic(@"", Environment.Default);

        [TestMethod]
        public void Okay()
            => VerifyDiagnostic(ReadText("Okay"), Environment.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            var startOffset = 12;
            DiagnosticResult Expected(int row, int col, string token)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = string.Format(
                    "The name '{0}' includes a underscore.",
                    token),
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col),
            };

            VerifyDiagnostic(
                code,
                Environment.Default,
                Expected(0, 17, "_alpha"),
                Expected(1, 17, "foo_bar"),
                Expected(2, 17, "_foo_bar_baz_"),
                Expected(5, 36, "_args"),
                Expected(7, 39, "_s"),
                Expected(11, 34, "_hello"),
                Expected(12, 25, "_action"),
                Expected(13, 19, "_n"),
                Expected(14, 30, "_action2"),
                Expected(15, 20, "_n"),
                Expected(15, 24, "_m"),
                Expected(16, 18, "_localFunc"),
                Expected(16, 33, "_v"),
                Expected(20, 32, "_o"));
            VerifyFix(code, fix);
        }
    }
}
