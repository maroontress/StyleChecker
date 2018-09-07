#pragma warning disable RS1012

namespace StyleChecker
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleChecker.Settings.InvalidConfig;

    /// <summary>
    /// Provides deserialized configuration data from <c>StyleChecker.xml</c>.
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        /// The name of configuration file.
        /// </summary>
        private static readonly string Filename = "StyleChecker.xml";

        /// <summary>
        /// The configuration file representing for this object.
        /// </summary>
        private readonly AdditionalText configFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        public Config()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        /// <param name="configFile">
        /// The configuration file representing for the instance.
        /// </param>
        public Config(AdditionalText configFile)
        {
            this.configFile = configFile;
            FilePath = configFile.Path;
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets the error argument.
        /// </summary>
        public string ErrorArgument { get; private set; }

        /// <summary>
        /// Gets the error position.
        /// </summary>
        public IXmlLineInfo ErrorPosition { get; private set; }

        /// <summary>
        /// Gets the path of configuration file;
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Gets or sets the maximum line length.
        /// </summary>
        /// <seealso cref="Size.LongLine.Analyzer"/>
        public int MaxLineLength { get; set; }

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
                var config = new Config(configFile);
                try
                {
                    Load(config, new MemoryStream(array));
                }
                catch (Exception e)
                {
                    config.ErrorMessage = e.Message;
                    config.ErrorArgument = e.ToString();
                    config.ErrorPosition = null;
                }
                finally
                {
                    action(config);
                }
            });
        }

        private static void Load(Config config, Stream stream)
        {
            var document = XDocument.Load(stream, LoadOptions.SetLineInfo);
            var root = document.Root;
            var rootName = root.Name;
            if (rootName.NamespaceName.Length != 0)
            {
                IXmlLineInfo info = root;
                config.ErrorMessage = Analyzer.InvalidNamespace;
                config.ErrorArgument = rootName.NamespaceName;
                config.ErrorPosition = info;
                return;
            }
            if (!rootName.LocalName.Equals("config"))
            {
                IXmlLineInfo info = root;
                config.ErrorMessage = Analyzer.InvalidRootElement;
                config.ErrorArgument = rootName.LocalName;
                config.ErrorPosition = info;
                return;
            }
            var maxLineLength = root.Attribute("maxLineLength");
            if (maxLineLength != null)
            {
                if (!int.TryParse(maxLineLength.Value, out var value))
                {
                    IXmlLineInfo info = maxLineLength;
                    config.ErrorMessage = Analyzer.InvalidMaxLineLength;
                    config.ErrorArgument = maxLineLength.Value;
                    config.ErrorPosition = info;
                    return;
                }
                config.MaxLineLength = value;
            }
        }
    }
}
