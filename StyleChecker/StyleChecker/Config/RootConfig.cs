#pragma warning disable CA1823

namespace StyleChecker.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using Maroontress.Oxbind;

    /// <summary>
    /// The root configuration.
    /// </summary>
    [ForElement("config", Namespace)]
    public sealed class RootConfig : AbstractConfig
    {
        [ElementSchema]
        private static readonly Schema TheSchema = Schema.Of(
            Optional.Of<ByteOrderMarkConfig>(),
            Optional.Of<DiscardingReturnValueConfig>(),
            Optional.Of<LongLineConfig>(),
            Optional.Of<ThoughtlessNameConfig>());

        /// <summary>
        /// Gets the configuration of LongLine analyzer.
        /// </summary>
        [field: ForChild]
        public ByteOrderMarkConfig ByteOrderMark { get; }
            = new ByteOrderMarkConfig();

        /// <summary>
        /// Gets the configuration of LongLine analyzer.
        /// </summary>
        [field: ForChild]
        public LongLineConfig LongLine { get; }
            = new LongLineConfig();

        /// <summary>
        /// Gets the configuration of ThoughtlessName analyzer.
        /// </summary>
        [field: ForChild]
        public ThoughtlessNameConfig ThoughtlessName { get; }
            = new ThoughtlessNameConfig();

        /// <summary>
        /// Gets the configuration of ThoughtlessName analyzer.
        /// </summary>
        [field: ForChild]
        public DiscardingReturnValueConfig DiscardingReturnValue { get; }
            = new DiscardingReturnValueConfig();

        /// <inheritdoc/>
        public override IEnumerable<(int, int, string)> Validate()
        {
            return Enumerables.Of<AbstractConfig>(
                    ByteOrderMark,
                    DiscardingReturnValue,
                    LongLine,
                    ThoughtlessName)
                .SelectMany(c => c.Validate());
        }
    }
}
