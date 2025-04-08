namespace StyleChecker.Test.Refactoring.NullCheckAfterDeclaration;

using Analyzers.Refactoring.NullCheckAfterDeclaration;
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
        => CheckNoDiagnostics("Okay");

    [TestMethod]
    public void TypeInferenceOkay()
        => CheckNoDiagnostics("TypeInferenceOkay");

    [TestMethod]
    public void LegacyOkay()
        => CheckNoDiagnostics("LegacyOkay");

    [TestMethod]
    public void LegacyTypeInferenceOkay()
        => CheckNoDiagnostics("LegacyTypeInferenceOkay");

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

    [TestMethod]
    public void AsExpr()
        => Check("AsExpr");

    private void CheckNoDiagnostics(string codeFile)
        => VerifyDiagnostic(ReadText(codeFile), Atmosphere.Default);

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
