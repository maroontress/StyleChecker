namespace StyleChecker.Cleaning.ByteOrderMark;

using System.Collections.Generic;

/// <summary>
/// Provides a way to manipulate directories.
/// </summary>
public interface DirectoryAct : FileAct
{
    /// <summary>
    /// Returns an enumerable collection of directory act in this
    /// directory.
    /// </summary>
    /// <returns>
    /// An enumerable collection of the directories in this directory.
    /// </returns>
    IEnumerable<DirectoryAct> GetDirectories();

    /// <summary>
    /// Returns an enumerable collection of file act in this directory.
    /// </summary>
    /// <returns>
    /// An enumerable collection of the files in this directory.
    /// </returns>
    IEnumerable<FileAct> GetFiles();
}
