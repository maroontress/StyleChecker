namespace StyleChecker.Test.Refactoring.DiscardingReturnValue
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Refactoring.DiscardingReturnValue;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine(Categories.Refactoring, "DiscardingReturnValue");

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var startOffset = 0;
            DiagnosticResult Expected(
                int row,
                int col,
                string name) => new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = $"The return value of '{name}' must be checked.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = SingleLocation(startOffset + row, col),
                };

            var read = "Read(byte[], int, int)";
            VerifyDiagnostic(
                code,
                Environment.Default,
                Expected(11, 13, $"System.IO.Stream.{read}"),
                Expected(16, 13, $"System.IO.BinaryReader.{read}"));
        }
    }
}
