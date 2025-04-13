namespace TestSuite.Refactoring.IneffectiveReadByte;

using BeliefCrucible;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Analyzers.Refactoring.IneffectiveReadByte;
using StyleChecker.CodeFixes.Refactoring.IneffectiveReadByte;

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
            var array = b.Message.Split(' ');
            var name = array[0];
            var arrayName = array[1];
            return b.ToResult(
                Analyzer.DiagnosticId,
                $"'{name}.ReadByte()' must be rewritten using "
                    + $"with '{name}.Read({arrayName}, int, int)'");
        }

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
