namespace TestSuite.Cleaning.UnusedUsing;

using System.Collections.Immutable;
using Analyzers.Cleaning.UnusedUsing;
using BeliefCrucible;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    public void Code()
    {
        var code = ReadText("Code");
        static Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            "The using directive is unused.");

        var ignoreIds = ImmutableArray.Create("CS8019");
        VerifyDiagnostic(
            code,
            Atmosphere.Default.WithExcludeIds(ignoreIds),
            Expected);
    }
}
