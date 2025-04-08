namespace Analyzers;

using System.Runtime.InteropServices;

/// <summary>
/// Provides platform-specfic utilities.
/// </summary>
public static class Platforms
{
    /// <summary>
    /// Gets the new line character sequence for the current platform.
    /// </summary>
    /// <returns>
    /// A string representing the new line character sequence for the current
    /// platform. On Windows, this is a carriage return followed by a line feed
    /// ("\r\n"). On other platforms, this is just a line feed ("\n").
    /// </returns>
    public static string NewLine()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "\r\n" : "\n";
    }
}
