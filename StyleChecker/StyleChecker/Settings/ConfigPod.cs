namespace StyleChecker.Settings;

using System;
using StyleChecker.Config;

/// <summary>
/// Provides information of the configuration file, errors at parsing the file
/// and the content of the file.
/// </summary>
/// <param name="rootConfig">
/// The RootConfig object.
/// </param>
/// <param name="exception">
/// The exception if any.
/// </param>
/// <param name="path">
/// The path of the configuration file.
/// </param>
public sealed class ConfigPod(
    RootConfig rootConfig, Exception? exception, string? path)
{
    /// <summary>
    /// Gets the RootConfig object.
    /// </summary>
    public RootConfig RootConfig { get; } = rootConfig;

    /// <summary>
    /// Gets the exception if any.
    /// </summary>
    public Exception? Exception { get; } = exception;

    /// <summary>
    /// Gets the path of the configuration file.
    /// </summary>
    public string? Path { get; } = path;
}
