namespace StyleChecker.Test.Document.StrayText
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Document.StrayText;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        public AnalyzerTest()
            : base(new Analyzer())
        {
        }

        [TestMethod]
        public void Empty()
            => VerifyDiagnostic("", Atmosphere.Default);

        [TestMethod]
        public void SingleLineOkay()
            => Okay("SingleLineOkay");

        [TestMethod]
        public void MultiLineOkay()
            => Okay("MultiLineOkay");

        [TestMethod]
        public void SingleLineCode()
        {
            var code = ReadText("SingleLineCode");
            static Result Expected(Belief b) => b.ToResult(
                Analyzer.DiagnosticId,
                m => $"The text '{b.Message}' is outside the XML tag.");

            VerifyDiagnostic(code, Atmosphere.Default, Expected);
        }

        [TestMethod]
        public void MultiLineCode()
        {
            var code = ReadText("MultiLineCode");
            static Result Expected(Belief b) => b.ToResult(
                Analyzer.DiagnosticId,
                m => $"The text '{b.Message}' is outside the XML tag.");

            VerifyDiagnostic(code, Atmosphere.Default, Expected);
        }

        private void Okay(string path)
            => VerifyDiagnostic(ReadText(path), Atmosphere.Default);
    }
}
