#pragma warning disable CA1815

namespace StyleChecker.Test.Framework;

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

/// <summary>
/// Stores information about a <c>Diagnostic</c> appearing in a source.
/// </summary>
public struct Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> struct.
    /// </summary>
    /// <param name="locations">
    /// The locations.
    /// </param>
    /// <param name="id">
    /// The analyzer's ID.
    /// </param>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="serverity">
    /// The severity.
    /// </param>
    public Result(
        ResultLocation[] locations,
        string id,
        string message,
        DiagnosticSeverity serverity = DiagnosticSeverity.Warning)
    {
        Locations = ImmutableArray.Create(
            locations ?? Array.Empty<ResultLocation>());
        Id = id;
        Message = message;
        Severity = serverity;
    }

    /// <summary>
    /// Gets the locations.
    /// </summary>
    public ImmutableArray<ResultLocation> Locations { get; }

    /// <summary>
    /// Gets the severity.
    /// </summary>
    public DiagnosticSeverity Severity { get; }

    /// <summary>
    /// Gets the analyzer's code.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the description message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the path of the first location.
    /// </summary>
    public string Path => Locations.Length > 0
        ? (Locations[0].Path ?? "")
        : "";

    /// <summary>
    /// Gets the line number of the first location.
    /// </summary>
    public int Line => Locations.Length > 0 ? Locations[0].Line : -1;

    /// <summary>
    /// Gets the column number of the first location.
    /// </summary>
    public int Column => Locations.Length > 0 ? Locations[0].Column : -1;
}
