namespace TestHelper
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// The compilation environment.
    /// </summary>
    public sealed class Environment
    {
        /// <summary>
        /// The immutable empty string array, representing for ignoring no
        /// diagnostics.
        /// </summary>
        public static readonly ImmutableArray<string> EmptyIds
            = ImmutableArray.Create<string>();

        /// <summary>
        /// The default environment.
        /// </summary>
        public static readonly Environment Default = new Environment();

        /// <summary>
        /// Initializes a new instance of the <see cref="Environment"/> class.
        /// </summary>
        public Environment()
        {
            ExcludeIds = EmptyIds;
            ConfigText = null;
        }

        /// <summary>
        /// Gets the configuration XML text.
        /// </summary>
        public string ConfigText { get; private set; }

        /// <summary>
        /// Gets all IDs of diagnostics to be ignored.
        /// </summary>
        public ImmutableArray<string> ExcludeIds { get; private set; }

        /// <summary>
        /// Returns a new environment with the specified exclude IDs.
        /// </summary>
        /// <param name="excludeIds">The exclude IDs.</param>
        /// <returns>The new environment.</returns>
        public Environment WithExcludeIds(ImmutableArray<string> excludeIds)
            => With(e => e.ExcludeIds = excludeIds);

        /// <summary>
        /// Returns a new environment with the specified configuration text.
        /// </summary>
        /// <param name="configText">The configuration text.</param>
        /// <returns>The new environment.</returns>
        public Environment WithConfigText(string configText)
            => With(e => e.ConfigText = configText);

        private Environment With(Action<Environment> update)
        {
            var clone = MemberwiseClone() as Environment;
            update(clone);
            return clone;
        }
    }
}
