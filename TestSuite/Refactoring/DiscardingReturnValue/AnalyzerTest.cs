namespace TestSuite.Refactoring.DiscardingReturnValue;

using System.Collections.Generic;
using Analyzers.Refactoring.DiscardingReturnValue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Test.Framework;

[TestClass]
public sealed class AnalyzerTest : DiagnosticVerifier
{
    public AnalyzerTest()
        : base(new Analyzer())
    {
    }

    [TestMethod]
    public void Okay()
        => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

    [TestMethod]
    public void Code()
    {
        var code = ReadText("Code");
        var map = new Dictionary<string, string>()
        {
            ["Read"] = "Read(byte[], int, int)",
        };
        Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            $"The return value of '{b.Substitute(k => map[k])}' must be "
                + "used.");

        VerifyDiagnostic(code, Atmosphere.Default, Expected);
    }

    [TestMethod]
    public void Methods()
    {
        var code = ReadText("Methods");
        var configText = ReadText("MethodsConfig", "xml");
        var atmosphere = Atmosphere.Default
            .WithConfigText(configText);
        static Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            m => $"The return value of '{m}' must be used.");

        VerifyDiagnostic(code, atmosphere, Expected);
    }
}
