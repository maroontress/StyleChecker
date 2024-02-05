#pragma warning disable CA1815

namespace StyleChecker.Test.Framework;

using System;
using Maroontress.Util;
using Microsoft.CodeAnalysis;
using StyleChecker.Refactoring;

/// <summary>
/// The belief is a metadata embedded with the source code as a comment,
/// which represents the expected diagnostics. It can be used to create the
/// diagnostic result.
/// </summary>
public struct Belief
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Belief"/> struct.
    /// </summary>
    /// <param name="row">
    /// The row.
    /// </param>
    /// <param name="column">
    /// The column.
    /// </param>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="deltaRow">
    /// The delta row.
    /// </param>
    public Belief(int row, int column, string message, int deltaRow)
    {
        Row = row;
        Column = column;
        Message = message;
        DeltaRow = deltaRow;
    }

    /// <summary>
    /// Gets the message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the row.
    /// </summary>
    public int Row { get; }

    /// <summary>
    /// Gets the delta row.
    /// </summary>
    public int DeltaRow { get; }

    /// <summary>
    /// Gets the column.
    /// </summary>
    private int Column { get; }

    /// <summary>
    /// Returns a new <see cref="Belief"/> with the specified row.
    /// </summary>
    /// <param name="newRow">
    /// The row.
    /// </param>
    /// <returns>
    /// A new belief.
    /// </returns>
    public Belief WithRow(int newRow)
        => new Belief(newRow, Column, Message, DeltaRow);

    /// <summary>
    /// Returns a new string representing the substituted message with the
    /// specified mapping function.
    /// </summary>
    /// <param name="map">
    /// A function to map a key to the value.
    /// </param>
    /// <returns>
    /// A new substituted message.
    /// </returns>
    public string Substitute(Func<string, string> map)
        => Texts.Substitute(Message, map);

    /// <summary>
    /// Returns a new result with the specified analyzer's ID and the
    /// message created with the specified function.
    /// </summary>
    /// <param name="id">
    /// The analyzer's ID.
    /// </param>
    /// <param name="toMessage">
    /// A function returns the result message with the belief message.
    /// </param>
    /// <param name="serverity">
    /// The severity.
    /// </param>
    /// <returns>
    /// The new result.
    /// </returns>
    public Result ToResult(
        string id,
        Func<string, string> toMessage,
        DiagnosticSeverity serverity = DiagnosticSeverity.Warning)
        => new Result(SingleLocation(), id, toMessage(Message), serverity);

    /// <summary>
    /// Returns a new result with the specified analyzer's ID and the
    /// specified message.
    /// </summary>
    /// <param name="id">
    /// The analyzer's ID.
    /// </param>
    /// <param name="message">
    /// The result message.
    /// </param>
    /// <param name="serverity">
    /// The severity.
    /// </param>
    /// <returns>
    /// The new result.
    /// </returns>
    public Result ToResult(
        string id,
        string message,
        DiagnosticSeverity serverity = DiagnosticSeverity.Warning)
        => new Result(SingleLocation(), id, message, serverity);

    /// <summary>
    /// Returns a new array of <see cref="ResultLocation"/> containing the
    /// single element representing the location of this belief.
    /// </summary>
    /// <returns>
    /// A new array of <see cref="ResultLocation"/> containing the single
    /// element.
    /// </returns>
    private ResultLocation[] SingleLocation()
    {
        return Arrays.Of(new ResultLocation("Test0.cs", Row, Column));
    }
}
