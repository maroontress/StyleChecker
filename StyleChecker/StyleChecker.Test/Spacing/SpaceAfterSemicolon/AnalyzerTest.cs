namespace StyleChecker.Test.Spacing.SpaceAfterSemicolon
{
    using System;
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
        [TestMethod]
        public void Empty()
        {
            VerifyCSharpDiagnostic(@"");
        }

        [TestMethod]
        public void Code()
        {
            var code = File.ReadAllText("Spacing/SpaceAfterSemicolon/Code.cs");
            var fix = File.ReadAllText("Spacing/SpaceAfterSemicolon/CodeFix.cs");
            Func<int, int, DiagnosticResult> expected
                = (col, row) => new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(
                        "A white space is needed after '{0}'", ";"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", col, row),
                    }
                };
            VerifyCSharpDiagnostic(
                code,
                expected(35, 34),
                expected(36, 34),
                expected(37, 27),
                expected(40, 35),
                expected(43, 18));
            VerifyCSharpFix(code, fix);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CodeFixer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new Analyzer();
        }
    }
}
