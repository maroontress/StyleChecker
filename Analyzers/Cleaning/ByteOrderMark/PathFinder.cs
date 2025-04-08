namespace Analyzers.Cleaning.ByteOrderMark;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

/// <summary>
/// Provides an ability to find files in a file tree.
/// </summary>
public static class PathFinder
{
    /// <summary>
    /// The default value of the maximum number of directory levels to search.
    /// </summary>
    public const int DefaultMaxDepth = 16;

    /// <summary>
    /// Returns an enumerable collection of file path in the specified
    /// directory and its subdirectories.
    /// </summary>
    /// <param name="root">
    /// The path of the directory that is the root of a file tree to search
    /// files.
    /// </param>
    /// <param name="depth">
    /// The maximum number of directory levels to search.
    /// </param>
    /// <returns>
    /// The enumerable collection of file path in the specified directory and
    /// its subdirectories.
    /// </returns>
    public static IEnumerable<string> GetFiles(
        string root, int depth = DefaultMaxDepth)
    {
        if (depth < 0)
        {
            throw new ArgumentException($"{nameof(depth)} is negative.");
        }
        var dir = Toolkit.TheInstance.GetDirectoryAct(root);
        return GetFilesRecursively(dir, root, depth);
    }

    private static IEnumerable<string> GetFilesRecursively(
          DirectoryAct dir, string root, int depth)
    {
        if (depth is 0)
        {
            return [];
        }

        static bool IsNormalFile(FileAct f)
        {
            var a = f.Attributes;
            return (a & FileAttributes.Hidden) is 0;
        }

        static bool IsNormalDirectory(DirectoryAct d)
        {
            var a = d.Attributes;
            return (a & FileAttributes.Hidden) is 0;
        }

        static IEnumerable<T> Of<T>(Func<IEnumerable<T>> action)
        {
            try
            {
                return action();
            }
            catch (DirectoryNotFoundException)
            {
                return [];
            }
            catch (SecurityException)
            {
                return [];
            }
        }

        string CombinePath(string n)
            => Path.Combine(root, n);

        var currentDirNames = Of(() => dir.GetFiles())
            .Where(IsNormalFile)
            .Select(f => CombinePath(f.Name));
        var subDirNames = Of(() => dir.GetDirectories())
            .Where(IsNormalDirectory)
            .SelectMany(d => GetFilesRecursively(d, d.Name, depth - 1))
            .Select(CombinePath);
        return currentDirNames.Concat(subDirNames);
    }
}
