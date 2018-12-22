namespace StyleChecker.Test.Spacing.SpaceBeforeSemicolon
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Spacing.SpaceBeforeSemicolon;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        public AnalyzerTest()
            : base(
                Path.Combine(Categories.Spacing, "SpaceBeforeSemicolon"),
                new Analyzer(),
                new CodeFixer())
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
                m => $"A white space is not needed before '{m}'");

            VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
        }
    }
}
