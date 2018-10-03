namespace StyleChecker.Test.Spacing.NoSpaceBeforeSemicolon
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Spacing.NoSpaceBeforeSemicolon;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override CodeFixProvider CodeFixProvider
            => new CodeFixer();

        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Spacing", "NoSpaceBeforeSemicolon");

        [TestMethod]
        public void Empty()
            => VerifyDiagnostic(@"", Environment.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            var startOffset = 17;
            DiagnosticResult Expected(int row, int col)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = string.Format(
                    "A white space is not needed before '{0}'", ";"),
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col),
            };
            VerifyDiagnostic(
                code,
                Environment.Default,
                Expected(0, 35),
                Expected(2, 17),
                Expected(4, 1),
                Expected(5, 28),
                Expected(8, 36),
                Expected(11, 26),
                Expected(14, 20),
                Expected(17, 19));
            VerifyFix(code, fix);
        }
    }
}
