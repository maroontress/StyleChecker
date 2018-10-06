namespace StyleChecker.Test.Naming.ThoughtlessName
{
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Naming.ThoughtlessName;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Naming", "ThoughtlessName");

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
            DiagnosticResult Expected(
                int row,
                int col,
                string token,
                System.Func<string, string, string> reason,
                string type) => new DiagnosticResult
            {
                Id = Analyzer.DiagnosticId,
                Message = $"The name '{token}' is too easy: "
                    + $"{reason(token, type)}",
                Severity = DiagnosticSeverity.Warning,
                Locations = SingleLocation(row, col),
            };

            string Arconym(string token, string type)
                => $"'{token}' is probably an acronym of its type name "
                    + $"'{type}'";
            DiagnosticResult ExpectedArconym(
                int row, int col, string token, string type)
                => Expected(row + 12, col, token, Arconym, type);

            string HungarianPrefix(string token, string type)
                => $"Hungarian notation is probably used for '{token}', "
                    + $"because the type name is '{type}'";
            DiagnosticResult ExpectedHungarianPrefix(
                int row, int col, string token, string type)
                => Expected(row + 18, col, token, HungarianPrefix, type);

            VerifyDiagnostic(
                code,
                Environment.Default,
                ExpectedArconym(0, 17, "sb", "StringBuilder"),
                ExpectedArconym(1, 17, "ms", "MemoryStream"),
                ExpectedHungarianPrefix(0, 17, "bIsTrue", "bool"),
                ExpectedHungarianPrefix(2, 17, "bRet", "byte"),
                ExpectedHungarianPrefix(3, 17, "sbRet", "sbyte"),
                ExpectedHungarianPrefix(5, 17, "iRet", "int"),
                ExpectedHungarianPrefix(6, 17, "uiRet", "uint"),
                ExpectedHungarianPrefix(8, 17, "sRet", "short"),
                ExpectedHungarianPrefix(9, 17, "usRet", "ushort"),
                ExpectedHungarianPrefix(11, 17, "lRet", "long"),
                ExpectedHungarianPrefix(12, 17, "ulRet", "ulong"),
                ExpectedHungarianPrefix(14, 17, "cRet", "char"),
                ExpectedHungarianPrefix(16, 17, "fRet", "float"),
                ExpectedHungarianPrefix(17, 17, "dRet", "double"),
                ExpectedHungarianPrefix(19, 17, "dPrice", "decimal"),
                ExpectedHungarianPrefix(20, 17, "sPrice", "string"),
                ExpectedHungarianPrefix(21, 17, "oPrice", "object"));
        }
    }
}
