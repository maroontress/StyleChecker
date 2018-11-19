namespace StyleChecker.Test.Refactoring.NotDesignedForExtenstion
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Refactoring.NotDesignedForExtension;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine(Categories.Refactoring, "NotDesignedForExtension");

        [TestMethod]
        public void Okay()
            => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            Result Expected(Belief b)
            {
                var m = b.Message.Split(",");
                var map = new Dictionary<string, (string, string)>
                {
                    ["m"] = ("Method", "abstract, sealed or empty"),
                    ["p"] = ("Property", "abstract or sealed"),
                };
                var (type, adjective) = map[m[0]];
                return b.ToResult(
                    Analyzer.DiagnosticId,
                    $"{type} '{m[1]}' is not designed for extension, "
                    + $"which needs to be {adjective}.");
            }

            VerifyDiagnostic(code, Atmosphere.Default, Expected);
        }
    }
}
