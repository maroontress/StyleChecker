namespace StyleChecker.Test.Cleaning.UnusedVariable
{
    using System.Collections.Immutable;
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Cleaning.UnusedVariable;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Cleaning", "UnusedVariable");

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
            var startOffset = 7;
            DiagnosticResult Expected(int row, int col, string name)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = $"The variable '{name}' is assigned but its value is never used.",
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col),
            };
            var ignoreIds = ImmutableArray.Create("CS0219");
            VerifyDiagnostic(
                code,
                Environment.Default.WithExcludeIds(ignoreIds),
                Expected(0, 25, "unused"),
                Expected(6, 17, "s"),
                Expected(9, 35, "unused"),
                Expected(15, 29, "s"),
                Expected(22, 48, "v"));
        }
    }
}
