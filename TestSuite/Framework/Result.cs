namespace StyleChecker.Test.Framework;

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

/// <summary>
/// Stores information about a <c>Diagnostic</c> appearing in a source.
/// </summary>
/// <param name="Locations">
/// The locations.
/// </param>
/// <param name="Id">
/// The analyzer's ID.
/// </param>
/// <param name="Message">
/// The message.
/// </param>
/// <param name="Severity">
/// The severity.
/// </param>
public readonly record struct Result(
    ImmutableArray<ResultLocation> Locations,
    string Id,
    string Message,
    DiagnosticSeverity Severity = DiagnosticSeverity.Warning)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> struct.
    /// </summary>
    /// <param name="array">
    /// The locations.
    /// </param>
    /// <param name="id">
    /// The analyzer's ID.</param>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="severity">
    /// The severity.
    /// </param>
    public Result(
        IEnumerable<ResultLocation> array,
        string id,
        string message,
        DiagnosticSeverity severity = DiagnosticSeverity.Warning)
        : this([.. array], id, message, severity)
    {
    }

    /// <summary>
    /// Gets the path of the first location.
    /// </summary>
    public string Path => Locations.Length > 0
        ? (Locations[0].Path ?? "")
        : "";

    /// <summary>
    /// Gets the line number of the first location.
    /// </summary>
    public int Line => Locations.Length > 0
        ? Locations[0].Line
        : -1;

    /// <summary>
    /// Gets the column number of the first location.
    /// </summary>
    public int Column => Locations.Length > 0
        ? Locations[0].Column
        : -1;
}
