namespace StyleChecker.Settings
{
    using System;
    using StyleChecker.Config;

    /// <summary>
    /// Provides information of the configuration file,
    /// errors at parsing the file and the content of the file.
    /// </summary>
    public sealed class ConfigPod
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigPod"/> class.
        /// </summary>
        /// <param name="rootConfig">
        /// The RootConfig object.
        /// </param>
        /// <param name="exception">
        /// The exception if any.
        /// </param>
        /// <param name="path">
        /// The path of the configuration file.
        /// </param>
        public ConfigPod(
            RootConfig rootConfig,
            Exception? exception,
            string? path)
        {
            RootConfig = rootConfig;
            Exception = exception;
            Path = path;
        }

        /// <summary>
        /// Gets the RootConfig object.
        /// </summary>
        public RootConfig RootConfig { get; }

        /// <summary>
        /// Gets the exception if any.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Gets the path of the configuration file.
        /// </summary>
        public string? Path { get; }
    }
}
