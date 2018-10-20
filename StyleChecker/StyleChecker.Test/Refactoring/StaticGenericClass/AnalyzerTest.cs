namespace StyleChecker.Test.Refactoring.StaticGenericClass
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Refactoring.StaticGenericClass;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override CodeFixProvider CodeFixProvider
            => new CodeFixer();

        protected override string BaseDir
            => Path.Combine(Categories.Refactoring, "StaticGenericClass");

        [TestMethod]
        public void Empty()
            => VerifyDiagnostic(@"", Environment.Default);

        [TestMethod]
        public void Okay()
            => VerifyDiagnostic(ReadText("Okay"), Environment.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            var startOffset = 5;
            DiagnosticResult Expected(int row, int col, string name)
                => new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = string.Format(
                    "Type parameters of the static class {0} "
                    + "must be moved to its methods.",
                    name),
                    Severity = DiagnosticSeverity.Warning,
                    Locations = SingleLocation(startOffset + row, col),
                };
            VerifyDiagnostic(
                code,
                Environment.Default,
                Expected(0, 25, "Code"),
                Expected(8, 25, "NoConstraintClause"),
                Expected(15, 25, "MultipleTypeParameter"),
                Expected(26, 25, "SingleLineDocumentationComment"),
                Expected(52, 25, "MultiLineDocumentationComment"));
            VerifyFix(code, fix);
        }

        [TestMethod]
        public void CodesWithReferences()
        {
            var codeChangeList = new[]
            {
                ReadCodeChange("ReferencedCode"),
                ReadCodeChange("ReferencingCode"),
            };
            VerifyFix(codeChangeList);
        }

        [TestMethod]
        public void MultiTypeParamCode()
        {
            var code = ReadText("MultiTypeParamCode");
            var fix = ReadText("MultiTypeParamCodeFix");
            VerifyFix(code, fix);
        }
    }
}
