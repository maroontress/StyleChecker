namespace StyleChecker.Test.Size.LongLine
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Size.LongLine;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        public AnalyzerTest()
            : base(Path.Combine(Categories.Size, "LongLine"), new Analyzer())
        {
        }

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
            Result Expected(Belief b)
                => b.ToResult(Analyzer.DiagnosticId, ToDetail);

            VerifyDiagnostic(code, Atmosphere.Default, Expected);
        }

        [TestMethod]
        public void DocumentComment()
        {
            var code = ReadText("DocumentComment");
            Result Expected(Belief b)
                => b.ToResult(Analyzer.DiagnosticId, ToDetail);

            VerifyDiagnostic(code, Atmosphere.Default, Expected);
        }

        [TestMethod]
        public void Config()
        {
            var code = ReadText("Code20");
            Result Expected(Belief b)
                => b.ToResult(Analyzer.DiagnosticId, ToDetail);

            var configText = ReadText("MaxLineLength20", "xml");
            VerifyDiagnostic(
                code,
                Atmosphere.Default.WithConfigText(configText),
                Expected);
        }

        private string ToDetail(string m)
            => $"The length of this line must be less than {m}.";
    }
}
