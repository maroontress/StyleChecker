namespace TestSuite.Refactoring.EqualsNull;

using Analyzers.Refactoring.EqualsNull;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Test.Framework;

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
