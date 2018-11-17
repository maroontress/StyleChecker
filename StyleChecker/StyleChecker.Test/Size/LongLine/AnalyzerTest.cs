namespace StyleChecker.Test.Size.LongLine
{
    using System.IO;
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
            => VerifyDiagnostic(@"", Atmosphere.Default);

        [TestMethod]
        public void Okay()
            => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            Result Expected(Belief b) => b.ToResult(
                Analyzer.DiagnosticId,
                m => $"The length of this line must be less than {m}.");

            VerifyDiagnostic(code, Atmosphere.Default, Expected);
        }

        [TestMethod]
        public void DocumentComment()
        {
            var code = ReadText("DocumentComment");
            Result Expected(Belief b) => b.ToResult(
                Analyzer.DiagnosticId,
                m => $"The length of this line must be less than {m}.");

            VerifyDiagnostic(code, Atmosphere.Default, Expected);
        }

        [TestMethod]
        public void Config()
        {
            var code = ReadText("Code20");
            Result Expected(Belief b) => b.ToResult(
                Analyzer.DiagnosticId,
                m => $"The length of this line must be less than {m}.");

            var configText = $"<config maxLineLength=\"20\" />";
            VerifyDiagnostic(
                code,
                Atmosphere.Default.WithConfigText(configText),
                Expected);
        }
    }
}
