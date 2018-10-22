namespace StyleChecker.Test.Refactoring.IneffectiveReadByte
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Refactoring.IneffectiveReadByte;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override CodeFixProvider CodeFixProvider
            => new CodeFixer();

        protected override string BaseDir
            => Path.Combine(Categories.Refactoring, "IneffectiveReadByte");

        [TestMethod]
        public void Okay()
            => VerifyDiagnostic(ReadText("Okay"), Environment.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            var fix = ReadText("CodeFix");
            var startOffset = 0;
            DiagnosticResult Expected(
                int row,
                int col,
                string name,
                string arrayName) => new DiagnosticResult
                {
                    Id = Analyzer.DiagnosticId,
                    Message = $"'{name}.ReadByte()' must be rewritten using "
                        + $"with '{name}.Read({arrayName}, int, int)'",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = SingleLocation(startOffset + row, col),
                };

            VerifyDiagnostic(
               code,
               Environment.Default,
               Expected(14, 13, "binaryReader", "byteArray"),
               Expected(25, 13, "reader", "buffer"),
               Expected(33, 13, "BinaryReaderProperty", "ByteArrayProperty"),
               Expected(41, 13, "reader", "array"));
            VerifyFix(code, fix);
        }
    }
}
