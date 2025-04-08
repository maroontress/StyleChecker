namespace Analyzers;

using System.IO;
using System.Reflection;

/// <summary>
/// Provides methods to have an access to the embedded resource.
/// </summary>
public static class EmbeddedResources
{
    /// <summary>
    /// Gets the text that the embedded resource contains.
    /// </summary>
    /// <param name="folder">
    /// The folder name.
    /// </param>
    /// <param name="filename">
    /// The file name.
    /// </param>
    /// <returns>
    /// The text that the embedded resource contains.
    /// </returns>
    /// <typeparam name="T">
    /// The type that is used to get the assembly.
    /// </typeparam>
    public static string GetText<T>(string folder, string filename)
        where T : class
    {
        var name = $"{folder}.{filename}";
        using var stream = typeof(T).GetTypeInfo()
            .Assembly
            .GetManifestResourceStream(name);
        return GetString(stream);
    }

    private static string GetString(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
