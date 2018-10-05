namespace StyleChecker.Test.Cleaning.UnusedUsing
{
    using System.Collections.Immutable;
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Cleaning.UnusedUsing;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Cleaning", "UnusedUsing");

        [TestMethod]
        public void Empty()
            => VerifyDiagnostic(@"", Environment.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var startOffset = 3;
            DiagnosticResult Expected(int row, int col)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = string.Format("The using directive is unused."),
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col),
            };
            var ignoreIds = ImmutableArray.Create("CS8019");
            VerifyDiagnostic(
                code,
                Environment.Default.WithExcludeIds(ignoreIds),
                Expected(0, 5));
        }
    }
}
