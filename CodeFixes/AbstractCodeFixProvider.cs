namespace CodeFixes;

using System.Globalization;
using Microsoft.CodeAnalysis.CodeFixes;

/// <summary>
/// The abstraction of code fix provider.
/// </summary>
#pragma warning disable RS1016 // Code fix providers should provide FixAll support
public abstract class AbstractCodeFixProvider : CodeFixProvider
#pragma warning restore RS1016 // Code fix providers should provide FixAll support
{
    /// <summary>
    /// Gets the compiler's culture.
    /// </summary>
    protected CultureInfo CompilerCulture { get; }
        /* https://github.com/dotnet/roslyn-analyzers/issues/7086 */
        = CultureInfo.GetCultureInfo("en-US");
}
