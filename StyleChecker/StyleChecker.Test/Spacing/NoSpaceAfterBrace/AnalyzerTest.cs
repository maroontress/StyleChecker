namespace StyleChecker.Test.Spacing.NoSpaceAfterBrace
{
    using Analyzers.Spacing.NoSpaceAfterBrace;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Spacing.NoSpaceAfterBrace;
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
            => VerifyDiagnostic("", Atmosphere.Default);

        [TestMethod]
        public void Okay()
        {
            var code = ReadText("Okay");
            VerifyDiagnostic(code, Atmosphere.Default);
        }

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var codeFix = ReadText("CodeFix");
            static Result Expected(Belief b) => b.ToResult(
                Analyzer.DiagnosticId,
                m => $"A white space is needed after '{m}'");

            VerifyDiagnosticAndFix(
                code, Atmosphere.Default, Expected, codeFix);
        }
    }
}
