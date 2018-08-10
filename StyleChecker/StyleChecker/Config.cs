#pragma warning disable RS1012
#pragma warning disable SA1401

namespace StyleChecker
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Provides deserialized configuration data from <c>StyleChecker.xml</c>.
    /// </summary>
    [XmlRoot("config")]
    public sealed class Config
    {
        /// <summary>
        /// Represents the maximum line length.
        /// </summary>
        /// <seealso cref="Size.LongLine.Analyzer"/>
        [XmlAttribute("maxLineLength")]
        public int MaxLineLength;

        /// <summary>
        /// The name of configuration file.
        /// </summary>
        private static readonly string Filename = "StyleChecker.xml";

        /// <summary>
        /// Registers the compilation start action with the specified context.
        /// The action reads the configuration file on the project root
        /// so to create a new <c>Config</c> instance with deserialization,
        /// and then perform the specified action with the new <c>Config</c>
        /// instance as a parameter.
        /// </summary>
        /// <param name="context">
        /// The context that registers compilation start action.
        /// </param>
        /// <param name="action">
        ///  The action to consume the deserialized <c>Config</c> object.
        /// </param>
        public static void Load(
            AnalysisContext context, Action<Config> action)
        {
            context.RegisterCompilationStartAction(c =>
            {
                var additionalFiles = c.Options.AdditionalFiles;
                var configFile = additionalFiles.FirstOrDefault(
                    f => Path.GetFileName(f.Path).Equals(Filename));
                if (configFile == null)
                {
                    return;
                }
                var text = configFile.GetText(c.CancellationToken);
                var stream = new MemoryStream();
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    text.Write(writer);
                }
                var array = stream.ToArray();
                var s = new XmlSerializer(typeof(Config));
                var config = (Config)s.Deserialize(new MemoryStream(array));
                action(config);
            });
        }
    }
}
