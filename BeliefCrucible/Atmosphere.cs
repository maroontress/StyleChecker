namespace BeliefCrucible;

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// The compilation environment.
/// </summary>
/// <param name="ExcludeIds">
/// The all IDs of diagnostics to be ignored.
/// </param>
/// <param name="BasePath">
/// The base path of source files.
/// </param>
/// <param name="ConfigText">
/// The configuration XML text.
/// </param>
/// <param name="ForceLocationValid">
/// The value indicating whether the location is forced to be valid.
/// </param>
/// <param name="DocumentationMode">
/// The documentation mode.
/// </param>
/// <param name="LangVersion">
/// The language version.
/// </param>
/// <remarks>
/// The filenames of the source files are automatically generated as
/// <c>Test0.cs</c>, <c>Test1.cs</c>, and so on. In general, these filenames
/// have no meaning, because almost all analyzers do not have direct access to
/// these files. So it is not even necessary that these files exist. However,
/// some analyzers (e.g., <c>Analyzers.Cleaning.ByteOrderMark.Analyzer</c>) do
/// have access to the source files specified by these paths. For these
/// analyzers, the base path represents the directory that actually contains
/// these files, such as <c>Test0.cs</c>.
/// </remarks>
public record class Atmosphere(
    ImmutableArray<string> ExcludeIds,
    string? BasePath,
    string? ConfigText,
    bool ForceLocationValid,
    DocumentationMode DocumentationMode,
    LanguageVersion LangVersion)
{
    /// <summary>
    /// The immutable empty string array, representing for ignoring no
    /// diagnostics.
    /// </summary>
    public static readonly ImmutableArray<string> EmptyIds = [];

    /// <summary>
    /// The default atmosphere.
    /// </summary>
    public static readonly Atmosphere Default = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Atmosphere"/> class.
    /// </summary>
    public Atmosphere()
        : this(
            EmptyIds,
            null,
            null,
            false,
            DocumentationMode.Parse,
            LanguageVersion.CSharp13)
    {
    }

    /// <summary>
    /// Returns a new atmosphere with the specified base path.
    /// </summary>
    /// <param name="basePath">
    /// The base path.
    /// </param>
    /// <returns>
    /// The new atmosphere.
    /// </returns>
    public Atmosphere WithBasePath(string basePath)
        => this with { BasePath = basePath };

    /// <summary>
    /// Returns a new atmosphere with the specified exclude IDs.
    /// </summary>
    /// <param name="excludeIds">
    /// The exclude IDs.
    /// </param>
    /// <returns>
    /// The new atmosphere.
    /// </returns>
    public Atmosphere WithExcludeIds(IEnumerable<string> excludeIds)
        => this with { ExcludeIds = [.. excludeIds] };

    /// <summary>
    /// Returns a new atmosphere with the specified exclude IDs.
    /// </summary>
    /// <param name="excludeIds">
    /// The exclude IDs.
    /// </param>
    /// <returns>
    /// The new atmosphere.
    /// </returns>
    public Atmosphere WithExcludeIds(params string[] excludeIds)
        => this with { ExcludeIds = [.. excludeIds] };

    /// <summary>
    /// Returns a new atmosphere with the specified configuration text.
    /// </summary>
    /// <param name="configText">
    /// The configuration text.
    /// </param>
    /// <returns>
    /// The new atmosphere.
    /// </returns>
    public Atmosphere WithConfigText(string configText)
        => this with { ConfigText = configText };

    /// <summary>
    /// Returns a new atmosphere with the specified boolean indicating whether
    /// the location is forced to be valid.
    /// </summary>
    /// <param name="force">
    /// Indicates whether the location is forced to be valid.
    /// </param>
    /// <returns>
    /// The new atmosphere.
    /// </returns>
    public Atmosphere WithForceLocationValid(bool force)
        => this with { ForceLocationValid = force };

    /// <summary>
    /// Returns a new atmosphere with the specified documentation mode.
    /// </summary>
    /// <param name="mode">
    /// The documentation mode.
    /// </param>
    /// <returns>
    /// The new atmosphere.
    /// </returns>
    public Atmosphere WithDocumentationMode(DocumentationMode mode)
        => this with { DocumentationMode = mode };

    /// <summary>
    /// Returns a new atmosphere with the specified language version.
    /// </summary>
    /// <param name="version">
    /// The language version.
    /// </param>
    /// <returns>
    /// The new atmosphere.
    /// </returns>
    public Atmosphere WithLangVersion(LanguageVersion version)
        => this with { LangVersion = version };
}
