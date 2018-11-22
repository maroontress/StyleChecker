namespace StyleChecker.Test.Refactoring.DiscardingReturnValue
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Refactoring.DiscardingReturnValue;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine(Categories.Refactoring, "DiscardingReturnValue");

        [TestMethod]
        public void Okay()
            => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var map = new Dictionary<string, string>()
            {
                ["Read"] = "Read(byte[], int, int)",
            };
            Result Expected(Belief b) => b.ToResult(
                Analyzer.DiagnosticId,
                $"The return value of '{b.Substitute(k => map[k])}' must be "
                    + "used.");

            VerifyDiagnostic(code, Atmosphere.Default, Expected);
        }
    }
}
