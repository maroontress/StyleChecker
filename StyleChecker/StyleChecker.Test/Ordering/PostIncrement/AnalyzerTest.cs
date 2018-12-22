namespace StyleChecker.Test.Ordering.PostIncrement
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Ordering.PostIncrement;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        public AnalyzerTest()
            : base(
                Path.Combine(Categories.Ordering, "PostIncrement"),
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
                m => $"The expression '{m}' must be replaced with the one "
                    + "using a pre-increment/decrement operator.");

            VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
        }
    }
}
