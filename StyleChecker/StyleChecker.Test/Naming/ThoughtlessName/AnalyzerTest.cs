namespace StyleChecker.Test.Naming.ThoughtlessName
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Naming.ThoughtlessName;
    using TestHelper;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Naming", "ThoughtlessName");

        [TestMethod]
        public void Empty()
            => VerifyCSharpDiagnostic(@"", Environment.Default);

        [TestMethod]
        public void Okay()
            => VerifyCSharpDiagnostic(ReadText("Okay"), Environment.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var startOffset = 10;
            string Arconym(string token, string type)
                => $"'{token}' is probably an acronym of its type name '{type}'";
            DiagnosticResult Expected(int row, int col, string token, string type)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = $"The name '{token}' is too easy: "
                    + $"{Arconym(token, type)}",
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col),
            };

            VerifyCSharpDiagnostic(
                code,
                Environment.Default,
                Expected(0, 17, "sb", "StringBuilder"),
                Expected(1, 17, "ms", "MemoryStream"));
        }
    }
}
