namespace TestSuite.Refactoring.TypeClassParameter;

using System.Collections.Frozen;
using System.Collections.Generic;
using BeliefCrucible;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Analyzers.Refactoring.TypeClassParameter;
using StyleChecker.CodeFixes.Refactoring.TypeClassParameter;

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
        => Check("Code");

    [TestMethod]
    public void RenameCode()
        => Check("RenameCode");

    [TestMethod]
    public void CodesWithReferences()
    {
        VerifyFix([
            "ReferencedCode",
            "ReferencingCode",
            ]);
    }

    private static Result Expected(Belief b)
    {
        var array = b.Message.Split(',');
        var parameterName = array[0];
        var functionKind = KindSet[array[1]];
        var functionName = array[2];
        return b.ToResult(
            Analyzer.DiagnosticId,
            $"""
            The parameter '{parameterName}' of the {functionKind} '{functionName}' must be replaced with the type parameter.
            """);
    }

    private void Check(string codeFile)
    {
        var change = NewCodeChange(codeFile);
        VerifyDiagnosticAndFix(change, Atmosphere.Default, Expected);
    }

    private void VerifyFix(IEnumerable<string> prefixes)
        => VerifyFix(prefixes.Select(NewCodeChange));
}
