namespace StyleChecker.Test.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Superclass of all Unit Tests for DiagnosticAnalyzers.
    /// </summary>
    public abstract class DiagnosticVerifier
    {
        private static readonly string NewLine = Environment.NewLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticVerifier"/>
        /// class.
        /// </summary>
        /// <param name="baseDir">
        /// The base directory to read files.
        /// </param>
        /// <param name="analyer">
        /// The diagnostic analyzer being tested.
        /// </param>
        protected DiagnosticVerifier(
            string baseDir, DiagnosticAnalyzer analyer)
        {
            BaseDir = baseDir;
            DiagnosticAnalyzer = analyer;
        }

        /// <summary>
        /// Gets the diagnostics analyzer being tested.
        /// </summary>
        /// <value>
        /// The diagnostics analyzer being tested.
        /// </value>
        protected DiagnosticAnalyzer DiagnosticAnalyzer { get; }

        /// <summary>
        /// Gets the base directory to read files.
        /// </summary>
        /// <value>
        /// The base directory to read files.
        /// </value>
        protected string BaseDir { get; }

        /// <summary>
        /// Returns a new array of <c>DiagnosticResultLocation</c> containing
        /// the single element representing the specified line and column.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="column">The column.</param>
        /// <returns>
        /// A new array of <c>DiagnosticResultLocation</c> containing
        /// the single element representing the specified line and column.
        /// </returns>
        protected static ResultLocation[] SingleLocation(
            int line, int column)
        {
            return Arrays.Create(
                new ResultLocation("Test0.cs", line, column));
        }

        /// <summary>
        /// Makes the test fail if <paramref name="b"/> is <c>false</c>.
        /// </summary>
        /// <param name="b">
        /// A <c>bool</c> value.
        /// </param>
        /// <param name="messageProvider">
        /// A function that returns the message to be a parameter of
        /// <see cref="Assert.Fail(string)"/>.
        /// </param>
        protected static void AssertFailIfFalse(
            bool b, Func<string> messageProvider)
        {
            if (b)
            {
                return;
            }
            Assert.Fail(messageProvider());
        }

        /// <summary>
        /// Makes the test fail if <paramref name="b"/> is <c>true</c>.
        /// </summary>
        /// <param name="b">
        /// A <c>bool</c> value.
        /// </param>
        /// <param name="messageProvider">
        /// A function that returns the message to be a parameter of
        /// <see cref="Assert.Fail(string)"/>.
        /// </param>
        protected static void AssertFailIfTrue(
            bool b, Func<string> messageProvider)
            => AssertFailIfFalse(!b, messageProvider);

        /// <summary>
        /// Verifies each of diagnostics found in the specified sources with
        /// the specified analyzer, compared with the specified expected
        /// result.
        /// </summary>
        /// <param name="sources">
        /// An array of strings to create source documents from to run the
        /// analyzers on.
        /// </param>
        /// <param name="analyzer">
        /// The analyzer to be run on the source code.
        /// </param>
        /// <param name="atmosphere">
        /// The compilation environment.
        /// </param>
        /// <param name="expected">
        /// The expected results that should appear after the analyzer is run
        /// on the sources.
        /// </param>
        protected static void VerifyDiagnostics(
            string[] sources,
            DiagnosticAnalyzer analyzer,
            Atmosphere atmosphere,
            params Result[] expected)
        {
            var documents = Projects.Of(sources).Documents.ToArray();
            if (sources.Length != documents.Length)
            {
                throw new InvalidOperationException(
                    "Amount of sources did not match amount of Documents "
                    + "created");
            }
            var diagnostics = Diagnostics.GetSorted(
                analyzer, documents, atmosphere);
            VerifyDiagnosticResults(
                diagnostics, analyzer, atmosphere, expected);
        }

        /// <summary>
        /// Gets the entire text representing for the specified file.
        /// </summary>
        /// <param name="name">
        /// The name of the source file to be read on the base directory.
        /// </param>
        /// <param name="ext">
        /// The extension of the file. The default value is <c>"cs"</c>.
        /// </param>
        /// <returns>
        /// The entire text representing for the specified file.
        /// </returns>
        protected string ReadText(string name, string ext = "cs")
        {
            var path = Path.Combine(BaseDir, $"{name}.{ext}");
            return File.ReadAllText(path);
        }

        /// <summary>
        /// Tests the analyzer. Verifies each of diagnostics found in the
        /// specified source, compared with the result the specified function
        /// extracts from the beliefs embedded from the source.
        /// </summary>
        /// <param name="encodedSource">
        /// The encoded source where the beliefs have been embedded.
        /// </param>
        /// <param name="atmosphere">
        /// The compilation environment.
        /// </param>
        /// <param name="toResult">
        /// The function that returns the expected diagnostic result with the
        /// specified belief.
        /// </param>
        protected void VerifyDiagnostic(
            string encodedSource,
            Atmosphere atmosphere,
            Func<Belief, Result> toResult)
        {
            var (source, expected) = Beliefs.Decode(
                encodedSource, atmosphere, toResult);
            VerifyDiagnostics(
                Arrays.Create(source),
                DiagnosticAnalyzer,
                atmosphere,
                expected);
        }

        /// <summary>
        /// Called to test a C# DiagnosticAnalyzer when applied on the single
        /// inputted string as a source Note: input a DiagnosticResult for each
        /// Diagnostic expected.
        /// </summary>
        /// <param name="source">
        /// A class in the form of a string to run the analyzer on.
        /// </param>
        /// <param name="atmosphere">
        /// The compilation environment.
        /// </param>
        /// <param name="expected">
        /// DiagnosticResults that should appear after the analyzer is run on
        /// the source.
        /// </param>
        protected void VerifyDiagnostic(
            string source,
            Atmosphere atmosphere,
            params Result[] expected)
        {
            VerifyDiagnostics(
                Arrays.Create(source),
                DiagnosticAnalyzer,
                atmosphere,
                expected);
        }

        /// <summary>
        /// Called to test a C# DiagnosticAnalyzer when applied on the inputted
        /// strings as a source Note: input a DiagnosticResult for each
        /// Diagnostic expected.
        /// </summary>
        /// <param name="sources">
        /// An array of strings to create source documents from to run the
        /// analyzers on.
        /// </param>
        /// <param name="atmosphere">
        /// The compilation environment.
        /// </param>
        /// <param name="expected">
        /// DiagnosticResults that should appear after the analyzer is run on
        /// the sources.
        /// </param>
        protected void VerifyDiagnostic(
            string[] sources,
            Atmosphere atmosphere,
            params Result[] expected)
        {
            VerifyDiagnostics(
                sources,
                DiagnosticAnalyzer,
                atmosphere,
                expected);
        }

        /// <summary>
        /// Checks each of the actual Diagnostics found and compares them with
        /// the corresponding DiagnosticResult in the array of expected
        /// results. Diagnostics are considered equal only if the
        /// DiagnosticResultLocation, Id, Severity, and Message of the
        /// DiagnosticResult match the actual diagnostic.
        /// </summary>
        /// <param name="actualDiagnostics">
        /// The Diagnostics found by the compiler after running the analyzer on
        /// the source code.
        /// </param>
        /// <param name="analyzer">
        /// The analyzer that was being run on the sources.
        /// </param>
        /// <param name="expectedResults">
        /// Diagnostic Results that should have appeared in the code.
        /// </param>
        private static void VerifyDiagnosticResults(
            IEnumerable<Diagnostic> actualDiagnostics,
            DiagnosticAnalyzer analyzer,
            Atmosphere atmosphere,
            params Result[] expectedResults)
        {
            var actualResults = actualDiagnostics.ToArray();
            var expectedCount = expectedResults.Count();
            var actualCount = actualResults.Count();

            string DiagnosticsOutput() => actualResults.Any()
                ? FormatDiagnostics(analyzer, atmosphere, actualResults)
                : "    NONE.";
            AssertFailIfFalse(
                expectedCount == actualCount,
                () => "Mismatch between number of diagnostics returned, "
                    + $"expected '{expectedCount}' "
                    + $"actual '{actualCount}'{NewLine}"
                    + $"{NewLine}"
                    + $"Diagnostics:{NewLine}"
                    + $"{DiagnosticsOutput()}{NewLine}");

            for (var i = 0; i < expectedResults.Length; ++i)
            {
                var actual = actualResults[i];
                var expected = expectedResults[i];
                string Message() => FormatDiagnostics(
                    analyzer, atmosphere, actual);

                if (expected.Line == -1 && expected.Column == -1)
                {
                    AssertFailIfFalse(
                        Location.None.Equals(actual.Location),
                        () => $"Expected:{NewLine}"
                            + $"A project diagnostic with No location{NewLine}"
                            + $"Actual:{NewLine}"
                            + $"{Message()}");
                }
                else
                {
                    VerifyDiagnosticLocation(
                        analyzer,
                        atmosphere,
                        actual,
                        actual.Location,
                        expected.Locations.First());
                    var additionalLocations
                        = actual.AdditionalLocations.ToArray();

                    var expectedAdditionalLocations
                        = expected.Locations.Length - 1;
                    var actualAdditionalLocations
                        = additionalLocations.Length;
                    AssertFailIfFalse(
                        expectedAdditionalLocations
                            == actualAdditionalLocations,
                        () => $"Expected "
                        + $"{expectedAdditionalLocations} "
                        + $"additional locations but got "
                        + $"{actualAdditionalLocations} for "
                        + $"Diagnostic:{NewLine}"
                        + $"    {Message()}{NewLine}");

                    for (var j = 0; j < additionalLocations.Length; ++j)
                    {
                        VerifyDiagnosticLocation(
                            analyzer,
                            atmosphere,
                            actual,
                            additionalLocations[j],
                            expected.Locations[j + 1]);
                    }
                }

                void AssertOne<T>(
                    string label, T expectedValue, T actualValue)
                {
                    if (expectedValue.Equals(actualValue))
                    {
                        return;
                    }
                    Assert.Fail(
                        $"Expected diagnostic {label} to be "
                        + $"'{expectedValue}' was "
                        + $"'{actualValue}'{NewLine}"
                        + $"{NewLine}"
                        + $"Diagnostic:{NewLine}"
                        + $"    {Message()}{NewLine}");
                }
                AssertOne("ID", expected.Id, actual.Id);
                AssertOne("severity", expected.Severity, actual.Severity);
                AssertOne("message", expected.Message, actual.GetMessage());
            }
        }

        /// <summary>
        /// Helper method to VerifyDiagnosticResult that checks the location of
        /// a diagnostic and compares it with the location in the expected
        /// DiagnosticResult.
        /// </summary>
        /// <param name="analyzer">
        /// The analyzer that was being run on the sources.
        /// </param>
        /// <param name="diagnostic">
        /// The diagnostic that was found in the code.
        /// </param>
        /// <param name="actual">
        /// The Location of the Diagnostic found in the code.
        /// </param>
        /// <param name="expected">
        /// The DiagnosticResultLocation that should have been found.
        /// </param>
        private static void VerifyDiagnosticLocation(
            DiagnosticAnalyzer analyzer,
            Atmosphere atmosphere,
            Diagnostic diagnostic,
            Location actual,
            ResultLocation expected)
        {
            string Message() => FormatDiagnostics(
                analyzer, atmosphere, diagnostic);
            var actualSpan = actual.GetLineSpan();
            var actualLinePosition = actualSpan.StartLinePosition;

            AssertFailIfFalse(
                actualSpan.Path == expected.Path
                    || (actualSpan.Path != null
                        && actualSpan.Path.Contains("Test0.")
                        && expected.Path.Contains("Test.")),
                () => $"Expected diagnostic to be in file '{expected.Path}' "
                    + "was actually in file "
                    + $"'{actualSpan.Path}'{NewLine}"
                    + $"{NewLine}"
                    + $"Diagnostic:{NewLine}"
                    + $"    {Message()}{NewLine}");

            // Only check line position if there is an actual line in the real
            // diagnostic
            AssertFailIfTrue(
                actualLinePosition.Line >= 0
                    && actualLinePosition.Line + 1 != expected.Line,
                () => "Expected diagnostic to be on line "
                    + $"'{expected.Line}' was actually on line "
                    + $"'{actualLinePosition.Line + 1}'{NewLine}"
                    + $"{NewLine}"
                    + $"Diagnostic:{NewLine}"
                    + $"    {Message()}{NewLine}");

            // Only check column position if there is an actual column position
            // in the real diagnostic
            AssertFailIfTrue(
                actualLinePosition.Character >= 0
                    && actualLinePosition.Character + 1 != expected.Column,
                () => "Expected diagnostic to start at column "
                    + $"'{expected.Column}' was actually at column "
                    + $"'{actualLinePosition.Character + 1}'{NewLine}"
                    + $"{NewLine}"
                    + $"Diagnostic:{NewLine}"
                    + $"    {Message()}{NewLine}");
        }

        /// <summary>
        /// Helper method to format a Diagnostic into an easily readable
        /// string.
        /// </summary>
        /// <param name="analyzer">
        /// The analyzer that this verifier tests.
        /// </param>
        /// <param name="diagnostics">
        /// The Diagnostics to be formatted.
        /// </param>
        /// <returns>
        /// The Diagnostics formatted as a string.
        /// </returns>
        private static string FormatDiagnostics(
            DiagnosticAnalyzer analyzer,
            Atmosphere atmosphere,
            params Diagnostic[] diagnostics)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < diagnostics.Length; ++i)
            {
                builder.AppendLine("// " + diagnostics[i].ToString());

                var analyzerType = analyzer.GetType();
                var rule = analyzer.SupportedDiagnostics
                    .Where(r => r != null && r.Id == diagnostics[i].Id)
                    .FirstOrDefault();
                if (rule == null)
                {
                    continue;
                }
                var location = diagnostics[i].Location;
                if (location == Location.None)
                {
                    builder.AppendFormat(
                        CultureInfo.CurrentCulture,
                        "GetGlobalResult({0}.{1})",
                        analyzerType.Name,
                        rule.Id);
                }
                else if (atmosphere.ForceLocationValid)
                {
                    var linePosition = diagnostics[i].Location
                        .GetLineSpan()
                        .StartLinePosition;
                    builder.AppendFormat(
                        CultureInfo.CurrentCulture,
                        "GetExternalResult({0}, {1}, {2}.{3})",
                        linePosition.Line + 1,
                        linePosition.Character + 1,
                        analyzerType.Name,
                        rule.Id);
                }
                else
                {
                    AssertFailIfFalse(
                        location.IsInSource,
                        () => "Test base does not currently handle "
                            + "diagnostics in metadata locations. "
                            + "Diagnostic in metadata: "
                            + $"{diagnostics[i]}{NewLine}");

                    var filePath = diagnostics[i].Location
                        .SourceTree
                        .FilePath;

                    AssertFailIfFalse(
                        filePath.EndsWith(".cs"),
                        () => "The file path does not end '.cs': "
                            + $"{filePath}");

                    var resultMethodName = "GetCSharpResultAt";
                    var linePosition = diagnostics[i].Location
                        .GetLineSpan()
                        .StartLinePosition;

                    builder.AppendFormat(
                        CultureInfo.CurrentCulture,
                        "{0}({1}, {2}, {3}.{4})",
                        resultMethodName,
                        linePosition.Line + 1,
                        linePosition.Character + 1,
                        analyzerType.Name,
                        rule.Id);
                }

                if (i != diagnostics.Length - 1)
                {
                    builder.Append(',');
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}
