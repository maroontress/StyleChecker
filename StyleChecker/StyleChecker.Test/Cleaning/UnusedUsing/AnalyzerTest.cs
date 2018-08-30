namespace StyleChecker.Test.Cleaning.UnusedUsing
{
    using System;
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Cleaning.UnusedUsing;
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
            var code = File.ReadAllText("Cleaning/UnusedUsing/Code.cs");
            var startOffset = 3;
            Func<int, int, DiagnosticResult> expected
                = (row, col) => new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(
                        "The using directive is unused."),
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation(
                            "Test0.cs", startOffset + row, col)
                    }
                };
            VerifyCSharpDiagnostic(
                code,
                expected(0, 5));
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new Analyzer();
        }
    }
}
