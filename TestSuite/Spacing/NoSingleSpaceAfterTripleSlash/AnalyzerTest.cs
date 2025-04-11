namespace TestSuite.Spacing.NoSingleSpaceAfterTripleSlash;

using Analyzers.Spacing.NoSingleSpaceAfterTripleSlash;
using BeliefCrucible;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Spacing.NoSingleSpaceAfterTripleSlash;

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
        var code = ReadText("Code");
        var fix = ReadText("CodeFix");
        static Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            m => $"A single white space is needed after '///'");

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
