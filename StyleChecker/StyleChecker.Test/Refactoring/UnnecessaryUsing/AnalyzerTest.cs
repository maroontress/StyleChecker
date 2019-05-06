namespace StyleChecker.Test.Refactoring.UnnecessaryUsing
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Refactoring.UnnecessaryUsing;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        public AnalyzerTest()
            : base(new Analyzer(), new CodeFixer())
        {
        }

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            Result Expected(Belief b) => b.ToResult(
                Analyzer.DiagnosticId,
                m => $"The using statement is not necessary for '{m}'.");

            VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
        }
    }
}
