namespace StyleChecker.Test.Framework
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// The compilation environment.
    /// </summary>
    public sealed class Atmosphere
    {
        /// <summary>
        /// The immutable empty string array, representing for ignoring no
        /// diagnostics.
        /// </summary>
        public static readonly ImmutableArray<string> EmptyIds
            = ImmutableArray.Create<string>();

        /// <summary>
        /// The default atmosphere.
        /// </summary>
        public static readonly Atmosphere Default = new Atmosphere();

        /// <summary>
        /// Initializes a new instance of the <see cref="Atmosphere"/> class.
        /// </summary>
        public Atmosphere()
        {
            ExcludeIds = EmptyIds;
            ConfigText = null;
            ForceLocationValid = false;
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
        /// Gets a value indicating whether the location is forced to be valid.
        /// </summary>
        public bool ForceLocationValid { get; private set; }

        /// <summary>
        /// Returns a new atmosphere with the specified exclude IDs.
        /// </summary>
        /// <param name="excludeIds">
        /// The exclude IDs.
        /// </param>
        /// <returns>
        /// The new atmosphere.
        /// </returns>
        public Atmosphere WithExcludeIds(ImmutableArray<string> excludeIds)
            => With(e => e.ExcludeIds = excludeIds);

        /// <summary>
        /// Returns a new atmosphere with the specified configuration text.
        /// </summary>
        /// <param name="configText">
        /// The configuration text.
        /// </param>
        /// <returns>
        /// The new atmosphere.
        /// </returns>
        public Atmosphere WithConfigText(string configText)
            => With(e => e.ConfigText = configText);

        /// <summary>
        /// Returns a new atmosphere with the specified boolean indicating
        /// whether the location is forced to be valid.
        /// </summary>
        /// <param name="force">
        /// Indicates whether the location is forced to be valid.
        /// </param>
        /// <returns>
        /// The new atmosphere.
        /// </returns>
        public Atmosphere WithForceLocationValid(bool force)
            => With(e => e.ForceLocationValid = force);

        private Atmosphere With(Action<Atmosphere> update)
        {
            var clone = MemberwiseClone() as Atmosphere;
            update(clone);
            return clone;
        }
    }
}
