namespace StyleChecker.Test.Naming.ThoughtlessName;

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Naming.ThoughtlessName;
using StyleChecker.Test.Framework;

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

        static Result ToResult(
            Belief b,
            string token,
            string type,
            Func<string, string, string> reason) => b.ToResult(
                Analyzer.DiagnosticId,
                $"The name '{token}' is too easy: {reason(token, type)}");

        static string ToSymbol(string token)
            => token.StartsWith('@') ? token.Substring(1) : token;

        static string Arconym(string token, string type)
            => $"'{ToSymbol(token)}' is probably an acronym of its type "
                + $"name '{type}'";

        static string HungarianPrefix(string token, string type)
            => "Hungarian notation is probably used for "
                + $"'{ToSymbol(token)}', because the type name is "
                + $"'{type}'";

        var map = new Dictionary<
            string, Func<Belief, string, string, Result>>
        {
            ["a"] = (b, n, t) => ToResult(b, n, t, Arconym),
            ["h"] = (b, n, t) => ToResult(b, n, t, HungarianPrefix),
        };

        Result Expected(Belief b)
        {
            var m = b.Message.Split(",");
            return map[m[0]](b, m[1], m[2]);
        }

        VerifyDiagnostic(code, Atmosphere.Default, Expected);
    }

    [TestMethod]
    public void Disallow()
    {
        var code = ReadText("Disallow");
        var configText = ReadText("DisallowFlagConfig", "xml");
        var atmosphere = Atmosphere.Default
            .WithConfigText(configText);

        static Result Expected(Belief b)
        {
            return b.ToResult(
                Analyzer.DiagnosticId,
                m => $"The name '{m}' is too easy: it is not allowed to "
                    + "use by the configuration file.");
        }

        VerifyDiagnostic(code, atmosphere, Expected);
    }
}
