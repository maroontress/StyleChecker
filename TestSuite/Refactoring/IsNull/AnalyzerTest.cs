namespace TestSuite.Refactoring.IsNull;

using Analyzers.Refactoring.IsNull;
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
