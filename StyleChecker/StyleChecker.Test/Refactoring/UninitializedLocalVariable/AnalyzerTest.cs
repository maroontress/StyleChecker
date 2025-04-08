namespace StyleChecker.Test.Refactoring.UninitializedLocalVariable;

using Analyzers.Refactoring.UninitializedLocalVariable;
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

    [TestMethod]
    public void Code()
    {
        var code = ReadText("Code");
        static Result Expected(Belief b)
        {
            return b.ToResult(
                Analyzer.DiagnosticId,
                $"Initialize '{b.Message}'.");
        }

        VerifyDiagnostic(code, Atmosphere.Default, Expected);
    }
}
