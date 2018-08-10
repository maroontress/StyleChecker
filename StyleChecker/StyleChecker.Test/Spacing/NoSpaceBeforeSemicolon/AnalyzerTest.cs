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
        [TestMethod]
        public void Empty()
        {
            VerifyCSharpDiagnostic(@"");
        }

        [TestMethod]
        public void Code()
        {
            var code = File.ReadAllText("Spacing/NoSpaceBeforeSemicolon/Code.cs");
            var fix = File.ReadAllText("Spacing/NoSpaceBeforeSemicolon/CodeFix.cs");
            Func<int, int, DiagnosticResult> expected
                = (col, row) => new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(
                        "A white space is not needed before '{0}'", ";"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", col, row),
                    }
                };
            VerifyCSharpDiagnostic(
                code,
                expected(15, 35),
                expected(17, 17),
                expected(19, 1),
                expected(20, 28),
                expected(23, 36),
                expected(26, 26),
                expected(29, 20),
                expected(32, 19));
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
