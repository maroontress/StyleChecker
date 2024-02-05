namespace StyleChecker.Test.Framework;

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

/// <summary>
/// Provides the utility methods for <c>Document</c>s.
/// </summary>
public static class Documents
{
    /// <summary>
    /// Gets the existing compiler diagnostics on the specified document.
    /// </summary>
    /// <param name="document">
    /// The <c>Document</c> to run the compiler diagnostic analyzers on.
    /// </param>
    /// <returns>
    /// The compiler diagnostics that were found in the code.
    /// </returns>
    public static IEnumerable<Diagnostic> GetCompilerDiagnostics(
        Document document)
    {
        var model = document.GetSemanticModelAsync().Result
            ?? throw new NullReferenceException();
        return model.GetDiagnostics();
    }
}
