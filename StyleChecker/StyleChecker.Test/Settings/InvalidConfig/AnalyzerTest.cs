namespace StyleChecker.Test.Settings.InvalidConfig
{
    using System;
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Settings.InvalidConfig;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        public AnalyzerTest()
            : base(
                Path.Combine(Categories.Settings, "InvalidConfig"),
                new Analyzer())
        {
        }

        [TestMethod]
        public void Empty()
            => VerifyDiagnostic(@"", Atmosphere.Default);

        [TestMethod]
        public void Okay()
        {
            var code = ReadText("Code");
            var configText = ReadText("OkayConfig", "xml");
            var atmosphere = Atmosphere.Default
                .WithForceLocationValid(true)
                .WithConfigText(configText);

            VerifyDiagnostic(code, atmosphere, Array.Empty<Result>());
        }

        [TestMethod]
        public void ConfigElementNotClosed()
        {
            var code = ReadText("Code");
            var configText = ReadText("ConfigElementNotClosed", "xml");
            var atmosphere = Atmosphere.Default
                .WithForceLocationValid(true)
                .WithConfigText(configText);

            ResultLocation[] NewLocations(int row, int col)
            {
                return new ResultLocation[]
                {
                    new ResultLocation("StyleChecker.xml", row, col),
                };
            }
            VerifyDiagnostic(
                code,
                atmosphere,
                new Result(
                    NewLocations(5, 1),
                    "InvalidConfig",
                    "StyleChecker.xml: Unexpected end of file has occurred. "
                    + "The following elements are not closed: "
                    + "config. Line 5, position 1.",
                    DiagnosticSeverity.Error));
        }

        [TestMethod]
        public void ValidationErrorOfLongLine()
        {
            var code = ReadText("Code");
            var configText = $"<config><LongLine maxLineLength=\"a\"/></config>";
            var atmosphere = Atmosphere.Default
                .WithForceLocationValid(true)
                .WithConfigText(configText);
            ResultLocation[] NewLocations(int row, int col)
            {
                return new ResultLocation[]
                {
                    new ResultLocation("StyleChecker.xml", row, col),
                };
            }
            VerifyDiagnostic(
                code,
                atmosphere,
                new Result(
                    NewLocations(0, 0),
                    "InvalidConfig",
                    "StyleChecker.xml: invalid integer value of "
                    + "maxLineLength attribute.",
                    DiagnosticSeverity.Error));
        }
    }
}
