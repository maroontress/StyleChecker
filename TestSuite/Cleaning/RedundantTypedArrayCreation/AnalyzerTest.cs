namespace TestSuite.Cleaning.RedundantTypedArrayCreation;

using Analyzers.Cleaning.RedundantTypedArrayCreation;
using BeliefCrucible;
using CodeFixes.Cleaning.RedundantTypedArrayCreation;
using Microsoft.CodeAnalysis.CSharp;
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
    public void Okay()
        => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

    [TestMethod]
    public void V9_Okay()
    {
        var atmosphere = Atmosphere.Default
            .WithLangVersion(LanguageVersion.CSharp9);
        VerifyDiagnostic(ReadText("C#9_Okay"), atmosphere);
    }

    [TestMethod]
    public void Code()
        => VerifyFix("Code");

    [TestMethod]
    public void MethodReference()
        => VerifyFix("MethodReference");

    private void VerifyFix(string file)
    {
        var code = ReadText(file);
        var fix = ReadText(file + "_Fixed");

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
