namespace StyleChecker.Test.Refactoring.StinkyBooleanExpression;

using Analyzers.Refactoring.StinkyBooleanExpression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Refactoring.StinkyBooleanExpression;
using StyleChecker.Test.Framework;

[TestClass]
public sealed class AnalyzerTest : CodeFixVerifier
{
    public AnalyzerTest()
        : base(new Analyzer(), new CodeFixer())
    {
    }

    [TestMethod]
    public void Okay()
        => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

    [TestMethod]
    public void Code()
    {
        var code = ReadText("Code");
        var fix = ReadText("CodeFix");
        static Result Expected(Belief b)
        {
            return b.ToResult(Analyzer.DiagnosticId, b.Message);
        }

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
