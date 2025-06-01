namespace TestSuite.Refactoring.StaticGenericClass;

using BeliefCrucible;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Analyzers.Refactoring.StaticGenericClass;
using StyleChecker.CodeFixes.Refactoring.StaticGenericClass;

[TestClass]
public sealed class AnalyzerTest : CodeFixVerifier
{
    public AnalyzerTest()
        : base(new Analyzer(), new CodeFixer())
    {
    }

    [TestMethod]
    public void Empty()
        => VerifyDiagnostic("", Atmosphere.Default);

    [TestMethod]
    public void Okay()
        => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

    [TestMethod]
    public void Code()
    {
        static Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            m => $"""
            Type parameters of the static class {m} must be moved to its methods.
            """);

        var change = NewCodeChange("Code");
        VerifyDiagnosticAndFix(change, Atmosphere.Default, Expected);
    }

    [TestMethod]
    public void CodesWithReferences()
    {
        VerifyFix([
            "ReferencedCode",
            "ReferencingCode",
            ]);
    }

    [TestMethod]
    public void MultiTypeParamCode()
    {
        var change = NewCodeChange("MultiTypeParamCode");
        VerifyFix(change);
    }

    [TestMethod]
    public void RenameCode()
    {
        var change = NewCodeChange("RenameCode");
        VerifyFix(change);
    }

    [TestMethod]
    public void RenameCodeWithReferences()
    {
        VerifyFix([
            "RenameReferencedCode",
            "RenameReferencingCode",
            ]);
    }

    private void VerifyFix(IEnumerable<string> prefixes)
        => VerifyFix(prefixes.Select(NewCodeChange));
}
