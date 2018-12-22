namespace StyleChecker.Test.Refactoring.StaticGenericClass
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Refactoring.StaticGenericClass;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        public AnalyzerTest()
            : base(
                Path.Combine(Categories.Refactoring, "StaticGenericClass"),
                new Analyzer(),
                new CodeFixer())
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
            var fix = ReadText("CodeFix");
            Result Expected(Belief b) => b.ToResult(
                Analyzer.DiagnosticId,
                m => "Type parameters of the static class "
                    + $"{m} must be moved to its methods.");

            VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
        }

        [TestMethod]
        public void CodesWithReferences()
        {
            var codeChangeList = new[]
            {
                ReadCodeChange("ReferencedCode"),
                ReadCodeChange("ReferencingCode"),
            };
            VerifyFix(codeChangeList);
        }

        [TestMethod]
        public void MultiTypeParamCode()
        {
            var code = ReadText("MultiTypeParamCode");
            var fix = ReadText("MultiTypeParamCodeFix");
            VerifyFix(code, fix);
        }
    }
}
