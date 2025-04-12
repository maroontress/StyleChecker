namespace TestSuite.Spacing.SpaceBeforeSemicolon;

using Analyzers.Spacing.SpaceBeforeSemicolon;
using BeliefCrucible;
using CodeFixes.Spacing.SpaceBeforeSemicolon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            m => $"A white space is not needed before '{m}'");

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
