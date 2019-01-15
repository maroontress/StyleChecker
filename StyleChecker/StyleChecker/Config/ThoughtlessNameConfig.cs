namespace StyleChecker.Config
{
    using System.Collections.Generic;
    using Maroontress.Oxbind;

    /// <summary>
    /// The configuration data of LongLine analyzer.
    /// </summary>
    [ForElement("ThoughtlessName", Namespace)]
    public sealed class ThoughtlessNameConfig : AbstractConfig
    {
        [ElementSchema]
        private static readonly Schema TheSchema = Schema.Of(
            Multiple.Of<Disallow>());

        /// <summary>
        /// Gets the identifiers that must not be used.
        /// </summary>
        /// <value>
        /// The identifiers that must not be used.
        /// </value>
        [field: ForChild]
        public IEnumerable<Disallow> DisallowList { get; }

        /// <inheritdoc/>
        public override IEnumerable<(int, int, string)> Validate()
            => NoError;

        /// <summary>
        /// Represents the identifier that must not be used.
        /// </summary>
        [ForElement("disallow", Namespace)]
        public class Disallow
        {
            /// <summary>
            /// Gets the identifier to be disallowed.
            /// </summary>
            [field: ForAttribute("id")]
            public string Id { get; }
        }
    }
}
