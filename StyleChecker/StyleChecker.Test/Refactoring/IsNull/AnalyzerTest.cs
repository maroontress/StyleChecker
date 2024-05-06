namespace StyleChecker.Test.Refactoring.IsNull;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Refactoring.IsNull;
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
