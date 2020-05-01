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
        public void Okay()
            => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            static Result Expected(Belief b) => b.ToResult(
                Analyzer.DiagnosticId,
                m => $"The text '{b.Message}' is outside the XML tag.");

            VerifyDiagnostic(code, Atmosphere.Default, Expected);
        }
    }
}
