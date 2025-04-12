namespace TestSuite.Cleaning.UnusedVariable;

using System.Collections.Generic;
using System.Collections.Immutable;
using Analyzers.Cleaning.UnusedVariable;
using BeliefCrucible;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roastery;

[TestClass]
public sealed class AnalyzerTest : DiagnosticVerifier
{
    public AnalyzerTest()
        : base(new Analyzer())
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
        var map = new Dictionary<string, string>
        {
            ["p"] = "parameter",
            ["v"] = "local variable",
            ["neverUsed"] = "its value is never used",
            ["usedButMarked"] = "its value is used but marked as unused",
            ["unnecessaryMark"] = "Unused attribute is not necessary",
        };
        Result Expected(Belief b)
        {
            var m = TextTemplates.Substitute(b.Message, k => map[k]).Split(',');
            var type = m[0];
            var name = m[1];
            var detail = m[2];
            return b.ToResult(
                Analyzer.DiagnosticId,
                $"The {type} '{name}': {detail}.");
        }
        var ignoreIds = ImmutableArray.Create("CS0219");
        VerifyDiagnostic(
            code,
            Atmosphere.Default.WithExcludeIds(ignoreIds),
            Expected);
    }
}
