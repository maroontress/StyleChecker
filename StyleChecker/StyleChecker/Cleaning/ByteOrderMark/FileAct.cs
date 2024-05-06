namespace StyleChecker.Cleaning.ByteOrderMark;

using System.IO;

/// <summary>
/// Provides a way to manipulate files.
/// </summary>
public interface FileAct
{
    /// <summary>
    /// Gets the name of this file.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the attributes for this file.
    /// </summary>
    FileAttributes Attributes { get; }
}
