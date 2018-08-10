namespace StyleChecker.Test.Ordering.PostIncrement
{
    using System;
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
        [TestMethod]
        public void Empty()
        {
            VerifyCSharpDiagnostic(@"");
        }

        [TestMethod]
        public void Code()
        {
            var code = File.ReadAllText("Ordering/PostIncrement/Code.cs");
            var fix = File.ReadAllText("Ordering/PostIncrement/CodeFix.cs");
            Func<int, int, string, DiagnosticResult> expected
                = (col, row, token) => new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(
                        "The expression '{0}' must be replaced with the one "
                        + "using a pre-increment/decrement operator.",
                        token),
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", col, row)
                    }
                };

            VerifyCSharpDiagnostic(
                code,
                expected(45, 13, "alpha++"),
                expected(46, 13, "beta--"),
                expected(47, 37, "k++"));
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
