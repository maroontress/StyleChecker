namespace StyleChecker.Test.Refactoring.AssignmentToParameter
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Refactoring.AssignmentToParameter;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine(Categories.Refactoring, "AssignmentToParameter");

        [TestMethod]
        public void Okay()
            => VerifyDiagnostic(ReadText("Okay"), Environment.Default);

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
                    Message = $"The assignment to the parameter '{name}' "
                        + "must be avoided.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = SingleLocation(startOffset + row, col),
                };
            VerifyDiagnostic(
                code,
                Environment.Default,
                Expected(7, 13, "value"),
                Expected(8, 13, "o"));
        }
    }
}
