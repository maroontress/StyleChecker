namespace StyleChecker.Test.Framework
{
    using System;
    using System.Collections.Generic;
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
        /// <summary>
        /// Gets the CSharp analyzer being tested - to be implemented in
        /// non-abstract class.
        /// </summary>
        protected abstract DiagnosticAnalyzer DiagnosticAnalyzer { get; }

        /// <summary>
        /// Gets the base directory to read files.
        /// </summary>
        /// <returns>
        /// The base directory.
        /// </returns>
        protected abstract string BaseDir { get; }

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
        protected static DiagnosticResultLocation[] SingleLocation(
            int line, int column)
        {
            return Arrays.Singleton(
                new DiagnosticResultLocation("Test0.cs", line, column));
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
        /// Gets the entire text representing for the specified file.
        /// </summary>
        /// <param name="name">
        /// The name of the source file to be read on the base directory.
        /// The extension ".cs" is not needed.
        /// </param>
        /// <returns>
        /// The entire text representing for the specified file.
        /// </returns>
        protected string ReadText(string name)
        {
            var path = Path.Combine(BaseDir, $"{name}.cs");
            return File.ReadAllText(path);
        }

        /// <summary>
        /// Called to test a C# DiagnosticAnalyzer when applied on the single
        /// inputted string as a source Note: input a DiagnosticResult for each
        /// Diagnostic expected.
        /// </summary>
        /// <param name="source">
        /// A class in the form of a string to run the analyzer on.
        /// </param>
        /// <param name="environment">
        /// The environment.
        /// </param>
        /// <param name="expected">
        /// DiagnosticResults that should appear after the analyzer is run on
        /// the source.
        /// </param>
        protected void VerifyDiagnostic(
            string source,
            Environment environment,
            params DiagnosticResult[] expected)
        {
            VerifyDiagnostics(
                Arrays.Singleton(source),
                DiagnosticAnalyzer,
                environment,
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
        /// <param name="environment">
        /// The environment.
        /// </param>
        /// <param name="expected">
        /// DiagnosticResults that should appear after the analyzer is run on
        /// the sources.
        /// </param>
        protected void VerifyDiagnostic(
            string[] sources,
            Environment environment,
            params DiagnosticResult[] expected)
        {
            VerifyDiagnostics(
                sources,
                DiagnosticAnalyzer,
                environment,
                expected);
        }

        /// <summary>
        /// General method that gets a collection of actual diagnostics found
        /// in the source after the analyzer is run, then verifies each of
        /// them.
        /// </summary>
        /// <param name="sources">
        /// An array of strings to create source documents from to run the
        /// analyzers on.
        /// </param>
        /// <param name="analyzer">
        /// The analyzer to be run on the source code.
        /// </param>
        /// <param name="environment">
        /// The environment.
        /// </param>
        /// <param name="expected">
        /// DiagnosticResults that should appear after the analyzer is run on
        /// the sources.
        /// </param>
        private static void VerifyDiagnostics(
            string[] sources,
            DiagnosticAnalyzer analyzer,
            Environment environment,
            params DiagnosticResult[] expected)
        {
            var documents = Projects.Of(sources).Documents.ToArray();
            if (sources.Length != documents.Length)
            {
                throw new InvalidOperationException(
                    "Amount of sources did not match amount of Documents "
                    + "created");
            }
            var diagnostics = Diagnostics.GetSorted(
                analyzer, documents, environment);
            VerifyDiagnosticResults(diagnostics, analyzer, expected);
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
            params DiagnosticResult[] expectedResults)
        {
            var actualResults = actualDiagnostics.ToArray();
            var expectedCount = expectedResults.Count();
            var actualCount = actualResults.Count();

            string DiagnosticsOutput() => actualResults.Any()
                ? FormatDiagnostics(analyzer, actualResults)
                : "    NONE.";
            AssertFailIfFalse(
                expectedCount == actualCount,
                () => "Mismatch between number of diagnostics returned, "
                    + $"expected '{expectedCount}' actual '{actualCount}'\r\n"
                    + "\r\n"
                    + "Diagnostics:\r\n"
                    + $"{DiagnosticsOutput()}\r\n");

            for (var i = 0; i < expectedResults.Length; ++i)
            {
                var actual = actualResults[i];
                var expected = expectedResults[i];
                string Message() => FormatDiagnostics(analyzer, actual);

                if (expected.Line == -1 && expected.Column == -1)
                {
                    AssertFailIfFalse(
                        Location.None.Equals(actual.Location),
                        () => "Expected:\r\n"
                            + "A project diagnostic with No location\r\n"
                            + "Actual:\r\n"
                            + $"{Message()}");
                }
                else
                {
                    VerifyDiagnosticLocation(
                        analyzer,
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
                        + $"{actualAdditionalLocations} for Diagnostic:\r\n"
                        + $"    {Message()}\r\n");

                    for (var j = 0; j < additionalLocations.Length; ++j)
                    {
                        VerifyDiagnosticLocation(
                            analyzer,
                            actual,
                            additionalLocations[j],
                            expected.Locations[j + 1]);
                    }
                }

                void AssertOne<T>(
                    string label,
                    T expectedValue,
                    T actualValue,
                    Func<string> messageProvider)
                {
                    if (expectedValue.Equals(actualValue))
                    {
                        return;
                    }
                    Assert.Fail(
                        $"Expected diagnostic {label} to be "
                        + $"'{expectedValue}' was "
                        + $"'{actualValue}'\r\n"
                        + "\r\n"
                        + "Diagnostic:\r\n"
                        + $"    {messageProvider()}\r\n");
                }
                AssertOne("ID", expected.Id, actual.Id, Message);
                AssertOne("severity", expected.Severity, actual.Severity, Message);
                AssertOne("message", expected.Message, actual.GetMessage(), Message);
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
            Diagnostic diagnostic,
            Location actual,
            DiagnosticResultLocation expected)
        {
            string Message() => FormatDiagnostics(analyzer, diagnostic);
            var actualSpan = actual.GetLineSpan();
            var actualLinePosition = actualSpan.StartLinePosition;

            AssertFailIfFalse(
                actualSpan.Path == expected.Path
                    || (actualSpan.Path != null
                        && actualSpan.Path.Contains("Test0.")
                        && expected.Path.Contains("Test.")),
                () => $"Expected diagnostic to be in file '{expected.Path}' "
                    + "was actually in file "
                    + $"'{actualSpan.Path}'\r\n"
                    + "\r\n"
                    + "Diagnostic:\r\n"
                    + $"    {Message()}\r\n");

            // Only check line position if there is an actual line in the real
            // diagnostic
            AssertFailIfTrue(
                actualLinePosition.Line > 0
                    && actualLinePosition.Line + 1 != expected.Line,
                () => "Expected diagnostic to be on line "
                    + $"'{expected.Line}' was actually on line "
                    + $"'{actualLinePosition.Line + 1}'\r\n"
                    + "\r\n"
                    + "Diagnostic:\r\n"
                    + $"    {Message()}\r\n");

            // Only check column position if there is an actual column position
            // in the real diagnostic
            AssertFailIfTrue(
                actualLinePosition.Character > 0
                    && actualLinePosition.Character + 1 != expected.Column,
                () => "Expected diagnostic to start at column "
                    + $"'{expected.Column}' was actually at column "
                    + $"'{actualLinePosition.Character + 1}'\r\n"
                    + "\r\n"
                    + "Diagnostic:\r\n"
                    + $"    {Message()}\r\n");
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
            DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics)
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
                        "GetGlobalResult({0}.{1})",
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
                            + $"{diagnostics[i]}\r\n");

                    var resultMethodName = diagnostics[i]
                        .Location
                        .SourceTree
                        .FilePath
                        .EndsWith(".cs")
                            ? "GetCSharpResultAt"
                            : "GetBasicResultAt";
                    var linePosition = diagnostics[i]
                        .Location
                        .GetLineSpan()
                        .StartLinePosition;

                    builder.AppendFormat(
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
