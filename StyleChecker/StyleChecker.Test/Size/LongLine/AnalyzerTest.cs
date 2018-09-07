namespace StyleChecker.Test.Size.LongLine
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Size.LongLine;
    using TestHelper;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Size", "LongLine");

        [TestMethod]
        public void Empty()
            => VerifyCSharpDiagnostic(@"", Environment.Default);

        [TestMethod]
        public void Okay()
            => VerifyCSharpDiagnostic(ReadText("Okay"), Environment.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var startOffset = 11;
            DiagnosticResult expected(int row, int col)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = string.Format(
                    "The length of this line must be less than {0}.",
                    "80"),
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col)
            };
            VerifyCSharpDiagnostic(
                code,
                Environment.Default,
                expected(0, 82));
        }
    }
}
