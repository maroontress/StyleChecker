namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Refactoring.NullCheckAfterDeclaration;
using StyleChecker.Test.Framework;

[TestClass]
public sealed class AnalyzerTest : CodeFixVerifier
{
    public AnalyzerTest()
        : base(new Analyzer(), new CodeFixer())
    {
    }

    [TestMethod]
    public void Okay()
        => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

    [TestMethod]
    public void TypeInferenceOkay()
        => VerifyDiagnostic(ReadText("TypeInferenceOkay"), Atmosphere.Default);

    [TestMethod]
    public void LegacyOkay()
        => VerifyDiagnostic(ReadText("LegacyOkay"), Atmosphere.Default);

    [TestMethod]
    public void LegacyTypeInferenceOkay()
        => VerifyDiagnostic(ReadText("LegacyTypeInferenceOkay"), Atmosphere.Default);

    [TestMethod]
    public void Code()
        => Check("Code");

    [TestMethod]
    public void LegacyCode()
        => Check("LegacyCode");

    [TestMethod]
    public void ConditionalExpr()
        => Check("ConditionalExpr");

    [TestMethod]
    public void CoalesceExpr()
        => Check("CoalesceExpr");

    private void Check(string codeFile, string? fixFile = null)
    {
        static Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            $"""
            Combine the declaration of '{b.Message}' and null check into a single pattern matching.
            """,
            DiagnosticSeverity.Info);

        var code = ReadText(codeFile);
        var fix = ReadText(fixFile ?? codeFile + "_Fixed");
        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
