namespace StyleChecker.Test.Size.LongLine
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Size.LongLine;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Size", "LongLine");

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
            var startOffset = 11;
            DiagnosticResult Expected(int row, int col)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = string.Format(
                    "The length of this line must be less than {0}.",
                    "80"),
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col),
            };
            VerifyDiagnostic(
                code,
                Environment.Default,
                Expected(0, 82));
        }

        [TestMethod]
        public void DocumentComment()
        {
            var code = ReadText("DocumentComment");
            var startOffset = 10;
            DiagnosticResult Expected(int row, int col)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = string.Format(
                    "The length of this line must be less than {0}.",
                    "80"),
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col),
            };
            VerifyDiagnostic(
                code,
                Environment.Default,
                Expected(0, 87));
        }

        [TestMethod]
        public void Config()
        {
            var code = ReadText("Code");
            var startOffset = 3;
            DiagnosticResult Expected(int row, int col)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = string.Format(
                    "The length of this line must be less than {0}.",
                    "20"),
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col),
            };
            var configText = $"<config maxLineLength=\"20\" />";
            VerifyDiagnostic(
                code,
                Environment.Default.WithConfigText(configText),
                Expected(0, 22));
        }
    }
}
