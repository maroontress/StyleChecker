namespace StyleChecker.Test.Spacing.SpaceAfterSemicolon
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Spacing.SpaceAfterSemicolon;
    using TestHelper;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override CodeFixProvider CSharpCodeFixProvider
            => new CodeFixer();

        protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Spacing", "SpaceAfterSemicolon");

        [TestMethod]
        public void Empty()
            => VerifyCSharpDiagnostic(@"", EmptyIds);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            var startOffset = 38;
            DiagnosticResult expected(int row, int col)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = string.Format(
                    "A white space is needed after '{0}'", ";"),
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col)
            };
            VerifyCSharpDiagnostic(
                code,
                EmptyIds,
                expected(0, 34),
                expected(1, 34),
                expected(2, 27),
                expected(5, 35),
                expected(8, 18));
            VerifyCSharpFix(code, fix);
        }
    }
}
