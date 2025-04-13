namespace StyleChecker.Analyzers;

using System.Globalization;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// The abstraction of analyzers.
/// </summary>
public abstract class AbstractAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Gets the compiler's culture.
    /// </summary>
    protected CultureInfo CompilerCulture { get; }
        /* https://github.com/dotnet/roslyn-analyzers/issues/7086 */
        = CultureInfo.GetCultureInfo("en-US");

    /// <inheritdoc/>
#pragma warning disable RS1026 // Enable concurrent execution
    public sealed override void Initialize(AnalysisContext context)
#pragma warning restore RS1026 // Enable concurrent execution
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        Register(context);
    }

    /// <summary>
    /// Resgisters actions with the specified <see
    /// cref="AnalysisContext"/>.
    /// </summary>
    /// <param name="context">
    /// The analysis context.
    /// </param>
    private protected abstract void Register(AnalysisContext context);
}
