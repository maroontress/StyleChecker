namespace StyleChecker.Test.Refactoring.EqualsNull
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Refactoring.EqualsNull;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        public AnalyzerTest()
            : base(
                Path.Combine(Categories.Refactoring, "EqualToNull"),
                new Analyzer(),
                new CodeFixer())
        {
        }

        [TestMethod]
        public void Okay()
            => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            Result Expected(Belief b)
            {
                var token = b.Message;
                return b.ToResult(
                    Analyzer.DiagnosticId,
                    $"Use 'is' pattern matching instead of "
                    + $"'{token}' operator.",
                    DiagnosticSeverity.Info);
            }

            VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
        }
    }
}
