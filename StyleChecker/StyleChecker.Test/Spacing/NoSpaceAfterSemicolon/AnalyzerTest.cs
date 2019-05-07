namespace StyleChecker.Test.Spacing.NoSpaceAfterSemicolon
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Spacing.NoSpaceAfterSemicolon;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        public AnalyzerTest()
            : base(new Analyzer(), new CodeFixer())
        {
        }

        [TestMethod]
        public void Empty()
            => VerifyDiagnostic(@"", Atmosphere.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            Result Expected(Belief b) => b.ToResult(
                Analyzer.DiagnosticId,
                m => $"A white space is needed after '{m}'");

            VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
        }
    }
}
