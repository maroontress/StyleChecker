namespace StyleChecker.Test.Refactoring.UnnecessaryUsing
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Refactoring.UnnecessaryUsing;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override CodeFixProvider CodeFixProvider
            => new CodeFixer();

        protected override string BaseDir
            => Path.Combine(Categories.Refactoring, "UnnecessaryUsing");

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            var startOffset = 0;
            DiagnosticResult Expected(
                int row,
                int col,
                string name) => new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = "The using statement is not necessary for "
                        + $"'{name}'.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = SingleLocation(startOffset + row, col),
                };

            VerifyDiagnostic(
               code,
               Environment.Default,
               Expected(9, 13, "s"),
               Expected(13, 13, "s"),
               Expected(18, 13, "s"),
               Expected(23, 13, "t"),
               Expected(28, 13, "t"),
               Expected(34, 13, "s"),
               Expected(40, 13, "s"),
               Expected(50, 13, "s"),
               Expected(52, 17, "t"),
               Expected(57, 13, "s"),
               Expected(66, 17, "t"),
               Expected(71, 13, "s"),
               Expected(75, 21, "u"));
            VerifyFix(code, fix);
        }
    }
}
