namespace StyleChecker.Test.Refactoring.StaticGenericClass
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Refactoring.StaticGenericClass;
    using TestHelper;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer
            => new Analyzer();

        protected override CodeFixProvider CSharpCodeFixProvider
            => new CodeFixer();

        protected override string BaseDir
            => Path.Combine(Categories.Refactoring, "StaticGenericClass");

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
            DiagnosticResult expected(int row, int col, string name)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = string.Format(
                    "Type parameters of the static class {0} "
                    + "must be moved to its methods.",
                    name),
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col)
            };
            VerifyCSharpDiagnostic(
                code,
                Environment.Default,
                expected(0, 25, "Code"),
                expected(8, 25, "NoConstraintClause"),
                expected(15, 25, "MultipleTypeParameter"));
            VerifyCSharpFix(code, fix);
        }
    }
}
