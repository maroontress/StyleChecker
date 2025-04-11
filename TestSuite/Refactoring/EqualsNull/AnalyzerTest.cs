namespace TestSuite.Refactoring.EqualsNull;

using Analyzers.Refactoring.EqualsNull;
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
}
