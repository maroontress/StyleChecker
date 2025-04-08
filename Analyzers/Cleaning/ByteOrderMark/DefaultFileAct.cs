namespace Analyzers.Cleaning.ByteOrderMark;

using System.IO;

/// <summary>
/// The default implementation of <see cref="FileAct"/> interface.
/// </summary>
public sealed class DefaultFileAct : FileAct
{
    private readonly FileInfo impl;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultFileAct"/> class.
    /// </summary>
    /// <param name="path">
    /// The path on which to create the <see cref="FileAct"/>.
    /// </param>
    public DefaultFileAct(string path)
        => impl = new FileInfo(path);

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultFileAct"/>
    /// class.
    /// </summary>
    /// <param name="info">
    /// The file information.
    /// </param>
    public DefaultFileAct(FileInfo info)
        => impl = info;

    /// <inheritdoc/>
    public string Name
        => impl.Name;

    /// <inheritdoc/>
    public FileAttributes Attributes
        => impl.Attributes;
}
