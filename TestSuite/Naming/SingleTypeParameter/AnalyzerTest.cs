namespace TestSuite.Naming.SingleTypeParameter;

using BeliefCrucible;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Analyzers.Naming.SingleTypeParameter;
using StyleChecker.CodeFixes.Naming.SingleTypeParameter;

[TestClass]
public sealed class AnalyzerTest : CodeFixVerifier
{
    public AnalyzerTest()
        : base(new Analyzer(), new CodeFixer())
    {
    }

    [TestMethod]
    public void Empty()
        => VerifyDiagnostic(@"", Atmosphere.Default);

    [TestMethod]
    public void Okay()
        => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

    [TestMethod]
    public void GoodTypes()
        => VerifyDiagnostic(ReadText("GoodTypes"), Atmosphere.Default);

    [TestMethod]
    public void GoodMembers()
        => VerifyDiagnostic(ReadText("GoodMembers"), Atmosphere.Default);

    [TestMethod]
    public void Code()
        => Check("Code");

    [TestMethod]
    public void BadTypes()
        => Check("BadTypes");

    [TestMethod]
    public void BadMembers()
        => Check("BadMembers");

    private void Check(string codeFile)
    {
        static Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            m => $"The type parameter name '{m}' is not 'T'.");

        var change = NewCodeChange(codeFile);
        VerifyDiagnosticAndFix(change, Atmosphere.Default, Expected);
    }
}
