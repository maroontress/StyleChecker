namespace StyleChecker.Test.Framework;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

/// <summary>
/// The compilation environment.
/// </summary>
public sealed class Atmosphere
{
    /// <summary>
    /// The immutable empty string array, representing for ignoring no
    /// diagnostics.
    /// </summary>
    public static readonly ImmutableArray<string> EmptyIds
        = ImmutableArray.Create<string>();

    /// <summary>
    /// The default atmosphere.
    /// </summary>
    public static readonly Atmosphere Default = new Atmosphere();

    /// <summary>
    /// Initializes a new instance of the <see cref="Atmosphere"/> class.
    /// </summary>
    public Atmosphere()
    {
        ExcludeIds = EmptyIds;
        BasePath = null;
        ConfigText = null;
        ForceLocationValid = false;
        DocumentationMode = DocumentationMode.Parse;
    }

    /// <summary>
    /// Gets the base path of source files.
    /// </summary>
    /// <remarks>
    /// The filenames of source files are generated automatically as
    /// <c>Test0.cs</c>, <c>Test1.cs</c>, and so on. In general, those
    /// filenames have no meaning because almost all analyzers do not have
    /// a direct access to those files. So it is not even necessary that
    /// those files exist. But some analyzers (e.g. ByteOrderMarker
    /// analyzer) exceptionally have an access to the source files
    /// specified with the those paths. For those analyzers, the base path
    /// represents the directory actually containing those files like
    /// <c>Test0.cs</c>.
    /// </remarks>
    public string? BasePath { get; private set; }

    /// <summary>
    /// Gets the configuration XML text.
    /// </summary>
    public string? ConfigText { get; private set; }

    /// <summary>
    /// Gets all IDs of diagnostics to be ignored.
    /// </summary>
    public ImmutableArray<string> ExcludeIds { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the location is forced to be valid.
    /// </summary>
    public bool ForceLocationValid { get; private set; }

    /// <summary>
    /// Gets the documentation mode.
    /// </summary>
    public DocumentationMode DocumentationMode { get; private set; }

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
        => With(e => e.BasePath = basePath);

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
        => With(e => e.ExcludeIds = excludeIds.ToImmutableArray());

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
        => WithExcludeIds(excludeIds as IEnumerable<string>);

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
        => With(e => e.ConfigText = configText);

    /// <summary>
    /// Returns a new atmosphere with the specified boolean indicating
    /// whether the location is forced to be valid.
    /// </summary>
    /// <param name="force">
    /// Indicates whether the location is forced to be valid.
    /// </param>
    /// <returns>
    /// The new atmosphere.
    /// </returns>
    public Atmosphere WithForceLocationValid(bool force)
        => With(e => e.ForceLocationValid = force);

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
        => With(e => e.DocumentationMode = mode);

    private Atmosphere With(Action<Atmosphere> update)
    {
        var clone = (Atmosphere)MemberwiseClone();
        update(clone);
        return clone;
    }
}
