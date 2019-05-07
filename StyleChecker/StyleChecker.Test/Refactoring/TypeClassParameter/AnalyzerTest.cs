namespace StyleChecker.Test.Refactoring.TypeClassParameter
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Refactoring.TypeClassParameter;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        public AnalyzerTest()
            : base(new Analyzer(), new CodeFixer())
        {
        }

        [TestMethod]
        public void Okay()
            => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

        [TestMethod]
        public void Code()
        {
            var kindSet = new Dictionary<string, string>()
            {
                ["L"] = "local function",
                ["M"] = "method",
            };
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            Result Expected(Belief b)
            {
                var array = b.Message.Split(',');
                var parameterName = array[0];
                var functionKind = kindSet[array[1]];
                var functionName = array[2];
                return b.ToResult(
                    Analyzer.DiagnosticId,
                    $"The parameter '{parameterName}' of the {functionKind} "
                    + $"'{functionName}' must be replaced with the type "
                    + $"parameter.");
            }

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
    }
}
