namespace StyleChecker.Test.Naming.SingleTypeParameter
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Naming.SingleTypeParameter;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override CodeFixProvider CodeFixProvider
            => new CodeFixer();

        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Naming", "SingleTypeParameter");

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
            var startOffset = 5;
            DiagnosticResult Expected(int row, int col, string token)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = string.Format(
                    "The type parameter name '{0}' is not 'T'.",
                    token),
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col),
            };

            VerifyDiagnostic(
                code,
                Environment.Default,
                Expected(0, 30, "Type"),
                Expected(6, 31, "Type"));
            VerifyFix(code, fix);
        }
    }
}
