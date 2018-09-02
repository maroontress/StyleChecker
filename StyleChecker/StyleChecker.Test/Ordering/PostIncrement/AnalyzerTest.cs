namespace StyleChecker.Test.Ordering.PostIncrement
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Ordering.PostIncrement;
    using TestHelper;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer
            => new Analyzer();

        protected override CodeFixProvider CSharpCodeFixProvider
            => new CodeFixer();

        protected override string BaseDir
            => Path.Combine("Ordering", "PostIncrement");

        [TestMethod]
        public void Empty()
            => VerifyCSharpDiagnostic(@"", EmptyIds);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            var startOffset = 45;
            DiagnosticResult expected(int row, int col, string token)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = string.Format(
                    "The expression '{0}' must be replaced with the one "
                    + "using a pre-increment/decrement operator.",
                    token),
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col)
            };

            VerifyCSharpDiagnostic(
                code,
                EmptyIds,
                expected(0, 13, "alpha++"),
                expected(1, 13, "beta--"),
                expected(2, 37, "k++"));
            VerifyCSharpFix(code, fix);
        }
    }
}
