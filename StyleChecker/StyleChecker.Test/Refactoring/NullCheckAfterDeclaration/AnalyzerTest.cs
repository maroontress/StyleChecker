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
    public void Code()
        => Check("Code", "CodeFix");

    [TestMethod]
    public void ConditionalExpr()
        => Check("ConditionalExpr", "ConditionalExpr_Fixed");

    [TestMethod]
    public void CoalesceExpr()
        => Check("CoalesceExpr", "CoalesceExpr_Fixed");

    private void Check(string codeFile, string fixFile)
    {
        static Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            $"""
            Combine the declaration of '{b.Message}' and null check into a single pattern matching.
            """,
            DiagnosticSeverity.Info);

        var code = ReadText(codeFile);
        var fix = ReadText(fixFile);
        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
