namespace TestSuite.Refactoring.TypeClassParameter;

using System.Collections.Frozen;
using System.Collections.Generic;
using Analyzers.Refactoring.TypeClassParameter;
using BeliefCrucible;
using CodeFixes.Refactoring.TypeClassParameter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class AnalyzerTest : CodeFixVerifier
{
    public AnalyzerTest()
        : base(new Analyzer(), new CodeFixer())
    {
    }

    private static IReadOnlyDictionary<string, string> KindSet { get; }
        = new Dictionary<string, string>()
            {
                ["L"] = "local function",
                ["M"] = "method",
            }.ToFrozenDictionary();

    [TestMethod]
    public void Okay()
        => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

    [TestMethod]
    public void Code()
    {
        var code = ReadText("Code");
        var fix = ReadText("CodeFix");

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }

    [TestMethod]
    public void RenameCode()
    {
        var code = ReadText("RenameCode");
        var fix = ReadText("RenameCodeFix");

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }

    [TestMethod]
    public void CodesWithReferences()
    {
        var codeChangeList = new[]
        {
            ReadCodeChange("ReferencedCode"),
            ReadCodeChange("ReferencingCode"),
        };
        VerifyFix(codeChangeList);
    }

    private static Result Expected(Belief b)
    {
        var array = b.Message.Split(',');
        var parameterName = array[0];
        var functionKind = KindSet[array[1]];
        var functionName = array[2];
        return b.ToResult(
            Analyzer.DiagnosticId,
            $"The parameter '{parameterName}' of the {functionKind} "
            + $"'{functionName}' must be replaced with the type "
            + $"parameter.");
    }
}
