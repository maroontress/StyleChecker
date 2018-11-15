namespace StyleChecker.Test.Cleaning.UnusedVariable
{
    using System.Collections.Immutable;
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Cleaning.UnusedVariable;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Cleaning", "UnusedVariable");

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
            var startOffset = 8;
            DiagnosticResult Expected(
                int row, int col, string type, string name, string detail)
                => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = $"The {type} '{name}': {detail}.",
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(startOffset + row, col),
            };
            var ignoreIds = ImmutableArray.Create("CS0219");
            var parameterType = "parameter";
            var variableType = "local variable";
            var neverUsed = "its value is never used";
            var usedButMarked = "its value is used but marked as unused";
            var unnecessaryMark = "Unused attribute is not necessary";
            VerifyDiagnostic(
                code,
                Environment.Default.WithExcludeIds(ignoreIds),
                Expected(0, 25, parameterType, "unused", neverUsed),
                Expected(6, 17, variableType, "s", neverUsed),
                Expected(9, 35, parameterType, "unused", neverUsed),
                Expected(15, 29, variableType, "s", neverUsed),
                Expected(22, 48, variableType, "v", neverUsed),
                Expected(27, 56, parameterType, "ignored", usedButMarked),
                Expected(32, 37, parameterType, "@baz", neverUsed),
                Expected(34, 17, variableType, "@foo", neverUsed),
                Expected(35, 33, variableType, "@stringFoo", neverUsed),
                Expected(38, 48, variableType, "@bar", neverUsed),
                Expected(46, 50, parameterType, "unused", unnecessaryMark),
                Expected(47, 57, parameterType, "unused", unnecessaryMark));
        }
    }
}
