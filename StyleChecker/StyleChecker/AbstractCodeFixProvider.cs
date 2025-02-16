namespace StyleChecker;

using System.Globalization;
using Microsoft.CodeAnalysis.CodeFixes;

/// <summary>
/// The abstraction of code fix provider.
/// </summary>
public abstract class AbstractCodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Gets the compiler's culture.
    /// </summary>
    protected CultureInfo CompilerCulture { get; }
        /* https://github.com/dotnet/roslyn-analyzers/issues/7086 */
        = CultureInfo.GetCultureInfo("en-US");
}
