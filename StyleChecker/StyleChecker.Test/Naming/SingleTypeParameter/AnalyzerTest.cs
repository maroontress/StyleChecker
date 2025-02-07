namespace StyleChecker.Test.Naming.SingleTypeParameter;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Naming.SingleTypeParameter;
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
        => VerifyDiagnostic(@"", Atmosphere.Default);

    [TestMethod]
    public void Okay()
        => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

    [TestMethod]
    public void GoodTypes()
        => VerifyDiagnostic(ReadText("GoodTypes"), Atmosphere.Default);

    [TestMethod]
    public void GoodMembers()
        => VerifyDiagnostic(ReadText("GoodMembers"), Atmosphere.Default);

    [TestMethod]
    public void Code()
        => Check("Code", "CodeFix");

    [TestMethod]
    public void BadTypes()
        => Check("BadTypes", "BadTypes_Fixed");

    [TestMethod]
    public void BadMembers()
        => Check("BadMembers", "BadMembers_Fixed");

    private void Check(string codeFile, string fixFile)
    {
        static Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            m => $"The type parameter name '{m}' is not 'T'.");

        var code = ReadText(codeFile);
        var fix = ReadText(fixFile);
        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
