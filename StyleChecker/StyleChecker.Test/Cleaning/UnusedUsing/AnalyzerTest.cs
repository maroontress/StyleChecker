namespace StyleChecker.Test.Cleaning.UnusedUsing
{
    using System.Collections.Immutable;
    using System.IO;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Cleaning.UnusedUsing;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Cleaning", "UnusedUsing");

        [TestMethod]
        public void Empty()
            => VerifyDiagnostic(@"", Atmosphere.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            Result Expected(Belief b) => b.ToResult(
                Analyzer.DiagnosticId,
                "The using directive is unused.");

            var ignoreIds = ImmutableArray.Create("CS8019");
            VerifyDiagnostic(
                code,
                Atmosphere.Default.WithExcludeIds(ignoreIds),
                Expected);
        }
    }
}
