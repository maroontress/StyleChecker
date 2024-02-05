namespace StyleChecker.Test.Cleaning.RedundantTypedArrayCreation;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Cleaning.RedundantTypedArrayCreation;
using StyleChecker.Test.Framework;

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

        static Result Expected(Belief b)
        {
            var all = b.Message.Split("|");
            var oldOne = all[0];
            var newOne = all[1];
            return b.ToResult(
                Analyzer.DiagnosticId,
                $"Replace '{oldOne}' with '{newOne}' to use an "
                + "implicitly-typed array creation.");
        }

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
