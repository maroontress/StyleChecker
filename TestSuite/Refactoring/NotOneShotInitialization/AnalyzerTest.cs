namespace TestSuite.Refactoring.NotOneShotInitialization;

using Analyzers.Refactoring.NotOneShotInitialization;
using BeliefCrucible;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            m => $"Declare '{m}' with one-shot initialization.");

        VerifyDiagnostic(code, Atmosphere.Default, Expected);
    }
}
