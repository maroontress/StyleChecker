namespace BeliefCrucible;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents compilation errors.
/// </summary>
public sealed class CompilationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompilationException"/>
    /// class.
    /// </summary>
    /// <param name="message">
    /// The error message.
    /// </param>
    /// <param name="rawDiagnostics">
    /// The raw diagnostics of the compiler.
    /// </param>
    public CompilationException(
            string message,
            IEnumerable<Diagnostic> rawDiagnostics)
            : base(NewMessage(message, rawDiagnostics))
        => RawDiagnostics = [.. rawDiagnostics];

    /// <summary>
    /// Initializes a new instance of the <see cref="CompilationException" />
    /// class.
    /// </summary>
    public CompilationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompilationException" />
    /// class.
    /// </summary>
    /// <param name="message">
    /// The message that describes the error.
    /// </param>
    public CompilationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompilationException" />
    /// class.
    /// </summary>
    /// <param name="message">
    /// The error message that explains the reason for the exception.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or a null
    /// reference if no inner exception is specified.
    /// </param>
    public CompilationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets the raw diagnostics of the compiler.
    /// </summary>
    public ImmutableArray<Diagnostic> RawDiagnostics { get; }

    private static string NewMessage(
        string message, IEnumerable<Diagnostic> rawDiagnostics)
    {
        var all = rawDiagnostics.Select(d => d.ToString())
            .Prepend(message);
        return string.Join(Environment.NewLine, all);
    }
}
