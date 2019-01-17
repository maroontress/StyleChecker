namespace StyleChecker.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Maroontress.Oxbind;

    /// <summary>
    /// The configuration data of ThoughtlessName analyzer.
    /// </summary>
    [ForElement("ThoughtlessName", Namespace)]
    public sealed class ThoughtlessNameConfig : AbstractConfig
    {
        [ElementSchema]
        private static readonly Schema TheSchema = Schema.Of(
            Multiple.Of<Disallow>());

        [field: ForChild]
        private IEnumerable<Disallow> DisallowElements { get; }
            = Array.Empty<Disallow>();

        /// <summary>
        /// Gets the identifiers that must not be used.
        /// </summary>
        /// <returns>
        /// The identifiers that must not be used.
        /// </returns>
        public IEnumerable<string> GetDisallowedIdentifiers()
        {
            return DisallowElements.Select(e => e.Id);
        }

        /// <inheritdoc/>
        public override IEnumerable<(int, int, string)> Validate()
            => NoError;

        /// <summary>
        /// Represents the identifier that must not be used.
        /// </summary>
        [ForElement("disallow", Namespace)]
        public sealed class Disallow
        {
            /// <summary>
            /// Gets the identifier to be disallowed.
            /// </summary>
            [field: ForAttribute("id")]
            public string Id { get; }
        }
    }
}