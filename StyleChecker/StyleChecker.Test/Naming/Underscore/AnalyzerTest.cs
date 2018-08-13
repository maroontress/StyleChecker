namespace StyleChecker.Test.Naming.Underscore
{
    using System;
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Naming.Underscore;
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
            var code = File.ReadAllText("Naming/Underscore/Code.cs");
            var fix = File.ReadAllText("Naming/Underscore/CodeFix.cs");
            Func<int, int, string, DiagnosticResult> expected
                = (col, row, token) => new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(
                        "The name '{0}' includes a underscore.",
                        token),
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", col, row)
                    }
                };

            VerifyCSharpDiagnostic(
                code,
                expected(13, 17, "_alpha"),
                expected(14, 17, "foo_bar"),
                expected(15, 17, "_foo_bar_baz_"),
                expected(20, 34, "_hello"));
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
