namespace TestSuite.Ordering.PostIncrement;

using BeliefCrucible;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Analyzers.Ordering.PostIncrement;
using StyleChecker.CodeFixes.Ordering.PostIncrement;

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
    public void Code()
    {
        var code = ReadText("Code");
        var fix = ReadText("CodeFix");

        static Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            m => $"The expression '{m}' must be replaced with the one "
                + "using a pre-increment/decrement operator.");

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
