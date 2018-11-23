namespace StyleChecker.Test.Naming.ThoughtlessName
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Naming.ThoughtlessName;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer
            => new Analyzer();

        protected override string BaseDir
            => Path.Combine("Naming", "ThoughtlessName");

        [TestMethod]
        public void Empty()
            => VerifyDiagnostic(@"", Atmosphere.Default);

        [TestMethod]
        public void Okay()
            => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

        [TestMethod]
        public void Code()
        {
            var code = ReadText("Code");
            Result ToResult(
                Belief b,
                string token,
                string type,
                Func<string, string, string> reason) => b.ToResult(
                    Analyzer.DiagnosticId,
                    $"The name '{token}' is too easy: {reason(token, type)}");

            string ToSymbol(string token)
                => token.StartsWith("@") ? token.Substring(1) : token;

            string Arconym(string token, string type)
                => $"'{ToSymbol(token)}' is probably an acronym of its type "
                    + $"name '{type}'";

            string HungarianPrefix(string token, string type)
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
    }
}
