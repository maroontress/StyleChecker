namespace StyleChecker.Test.Naming.Underscore;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Naming.Underscore;
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

        static Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            m => m is "_"
                ? $"The name '_' is just an underscore, not a discard."
                : $"The name '{m}' includes an underscore.");

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
