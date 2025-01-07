namespace StyleChecker.Test.Refactoring.NoUsingDeclaration;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Refactoring.NoUsingDeclaration;
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
            return b.ToResult(
                Analyzer.DiagnosticId,
                $"Insert 'using' before '{b.Message}'.");
        }

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
