namespace StyleChecker
{
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Provides methods to have an access to the embedded resource.
    /// </summary>
    public static class EmbeddedResources
    {
        private static string GetString(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

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
        public static string GetText(string folder, string filename)
        {
            var name = $"StyleChecker.{folder}.{filename}";
            using (var stream = typeof(EmbeddedResources).GetTypeInfo()
                .Assembly
                .GetManifestResourceStream(name))
            {
                return GetString(stream);
            }
        }
    }
}
