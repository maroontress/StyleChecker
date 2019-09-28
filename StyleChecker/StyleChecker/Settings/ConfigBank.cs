#pragma warning disable RS1012

namespace StyleChecker.Settings
{
    using System;
    using System.IO;
    using System.Linq;
    using Maroontress.Oxbind;
    using Maroontress.Util;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleChecker.Config;

    /// <summary>
    /// Provides the configuration data.
    /// </summary>
    public static class ConfigBank
    {
        /// <summary>
        /// The name of configuration file.
        /// </summary>
        public const string Filename = "StyleChecker.xml";

        private static readonly RootConfig DefaultRootConfig
            = new RootConfig();

        private static readonly WeakValueMap<string, ConfigPod> PodMap
            = new WeakValueMap<string, ConfigPod>();

        /// <summary>
        /// Gets the configuration pod associated with the specified context.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The configuration pod.
        /// </returns>
        public static ConfigPod LoadRootConfig(
            CompilationStartAnalysisContext context)
        {
            var (path, source) = LoadConfigFile(context);
            if (path is null || source is null)
            {
                return new ConfigPod(DefaultRootConfig, null, null);
            }
            lock (PodMap)
            {
                var pod = PodMap.Get(source);
                if (pod is null)
                {
                    pod = NewRootConfig(path, source);
                    PodMap.Put(source, pod);
                }
                return pod;
            }
        }

        /// <summary>
        /// Register the action invoked with the configuration pod,
        /// when the compilation starts.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="action">
        /// The action that supplies the configuration pod.
        /// </param>
        public static void LoadRootConfig(
            AnalysisContext context,
            Action<CompilationStartAnalysisContext, ConfigPod> action)
        {
            void StartAction(CompilationStartAnalysisContext c)
                => action(c, LoadRootConfig(c));

            context.RegisterCompilationStartAction(StartAction);
        }

        private static ConfigPod NewRootConfig(
            string path, string source)
        {
            try
            {
                var reader = new StringReader(source);
                var factory = new OxbinderFactory();
                var decoder = factory.Of<RootConfig>();
                var rootConfig = decoder.NewInstance(reader);
                return new ConfigPod(rootConfig, null, path);
            }
            catch (Exception e)
            {
                return new ConfigPod(DefaultRootConfig, e, path);
            }
        }

        private static (string? path, string? source) LoadConfigFile(
            CompilationStartAnalysisContext c)
        {
            var additionalFiles = c.Options.AdditionalFiles;
            var configFile = additionalFiles.FirstOrDefault(
                f => Path.GetFileName(f.Path) is Filename);
            if (configFile is null)
            {
                return (null, null);
            }
            var path = configFile.Path;
            var text = configFile.GetText(c.CancellationToken);
            var writer = new StringWriter();
            text.Write(writer);
            var source = writer.ToString();
            return (path, source);
        }
    }
}
