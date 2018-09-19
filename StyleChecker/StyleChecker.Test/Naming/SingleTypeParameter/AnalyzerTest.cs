namespace StyleChecker.Test.Naming.SingleTypeParameter
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Naming.SingleTypeParameter;
    using TestHelper;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override CodeFixProvider CSharpCodeFixProvider
            => new CodeFixer();

        protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Naming", "SingleTypeParameter");

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
            var fix = ReadText("CodeFix");
            var startOffset = 5;
            DiagnosticResult Expected(int row, int col, string token)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = string.Format(
                    "The type parameter name '{0}' is not 'T'.",
                    token),
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col),
            };

            VerifyCSharpDiagnostic(
                code,
                Environment.Default,
                Expected(0, 30, "Type"),
                Expected(6, 31, "Type"));
            VerifyCSharpFix(code, fix);
        }
    }
}
