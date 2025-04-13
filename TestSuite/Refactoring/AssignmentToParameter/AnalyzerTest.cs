namespace TestSuite.Refactoring.AssignmentToParameter;

using BeliefCrucible;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Analyzers.Refactoring.AssignmentToParameter;

[TestClass]
public sealed class AnalyzerTest : DiagnosticVerifier
{
    public AnalyzerTest()
        : base(new Analyzer())
    {
    }

    [TestMethod]
    public void Okay()
        => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

    [TestMethod]
    public void Code()
    {
        var code = ReadText("Code");
        static Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            m => $"The assignment to the parameter '{m}' must be "
                + "avoided.");

        VerifyDiagnostic(code, Atmosphere.Default, Expected);
    }
}
