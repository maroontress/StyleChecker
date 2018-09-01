namespace StyleChecker.Test.Spacing.NoSpaceBeforeSemicolon
{
    using System;
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Spacing.NoSpaceBeforeSemicolon;
    using TestHelper;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override CodeFixProvider CSharpCodeFixProvider
            => new CodeFixer();

        protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Spacing", "NoSpaceBeforeSemicolon");

        [TestMethod]
        public void Empty()
        {
            VerifyCSharpDiagnostic(@"", EmptyIds);
        }

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            var startOffset = 17;
            Func<int, int, DiagnosticResult> expected
                = (row, col) => new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(
                        "A white space is not needed before '{0}'", ";"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations = SingleLocation(startOffset + row, col)
                };
            VerifyCSharpDiagnostic(
                code,
                EmptyIds,
                expected(0, 35),
                expected(2, 17),
                expected(4, 1),
                expected(5, 28),
                expected(8, 36),
                expected(11, 26),
                expected(14, 20),
                expected(17, 19));
            VerifyCSharpFix(code, fix);
        }
    }
}
