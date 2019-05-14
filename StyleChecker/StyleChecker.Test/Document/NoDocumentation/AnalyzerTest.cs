namespace StyleChecker.Test.Document.NoDocumentation
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Document.NoDocumentation;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        private ImmutableArray<string> ignoreIds
            = ImmutableArray.Create("CS1591");

        public AnalyzerTest()
            : base(new Analyzer())
        {
        }

        [TestMethod]
        public void Empty()
            => VerifyDiagnostic(@"", Atmosphere.Default);

        [TestMethod]
        public void Okay()
        {
            var configText = ReadText("Okay", "xml");
            VerifyDiagnostic(
                ReadText("Okay"),
                Atmosphere.Default
                    .WithExcludeIds(ignoreIds)
                    .WithConfigText(configText)
                    .WithDocumentationMode(DocumentationMode.Diagnose));
        }

        [TestMethod]
        public void AllOkay()
        {
            var configText = ReadText("Okay", "xml");
            VerifyDiagnostic(
                ReadText("AllOkay"),
                Atmosphere.Default
                    .WithExcludeIds(ignoreIds)
                    .WithConfigText(configText)
                    .WithDocumentationMode(DocumentationMode.Diagnose));
        }

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            Result Expected(Belief b) => b.ToResult(
                Analyzer.DiagnosticId,
                "Missing XML comment for publicly visible type or member "
                + $"'{b.Message}'");

            VerifyDiagnostic(
                code,
                Atmosphere.Default
                    .WithExcludeIds(ignoreIds)
                    .WithDocumentationMode(DocumentationMode.Diagnose),
                Expected);
        }
    }
}
