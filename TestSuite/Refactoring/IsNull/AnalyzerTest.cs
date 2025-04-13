namespace TestSuite.Refactoring.IsNull;

using BeliefCrucible;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Analyzers.Refactoring.IsNull;

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
}
