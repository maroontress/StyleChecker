namespace Analyzers.Config;

using System.Collections.Generic;
using System.Linq;
using CodeDebt.Util;
using Maroontress.Oxbind;

/// <summary>
/// The root configuration.
/// </summary>
[ForElement("config", Namespace)]
public sealed class RootConfig : AbstractConfig
{
#pragma warning disable IDE0052 // Remove unread private members
    [ElementSchema]
    private static readonly Schema TheSchema = Schema.Of(
        Optional.Of<ByteOrderMarkConfig>(),
        Optional.Of<DiscardingReturnValueConfig>(),
        Optional.Of<LongLineConfig>(),
        Optional.Of<NoDocumentationConfig>(),
        Optional.Of<ThoughtlessNameConfig>());
#pragma warning restore IDE0052 // Remove unread private members

    /// <summary>
    /// Gets the configuration of LongLine analyzer.
    /// </summary>
    [field: ForChild]
    public ByteOrderMarkConfig ByteOrderMark { get; }
        = new ByteOrderMarkConfig();

    /// <summary>
    /// Gets the configuration of ThoughtlessName analyzer.
    /// </summary>
    [field: ForChild]
    public DiscardingReturnValueConfig DiscardingReturnValue { get; }
        = new DiscardingReturnValueConfig();

    /// <summary>
    /// Gets the configuration of LongLine analyzer.
    /// </summary>
    [field: ForChild]
    public LongLineConfig LongLine { get; }
        = new LongLineConfig();

    /// <summary>
    /// Gets the configuration of NoDocumentation analyzer.
    /// </summary>
    [field: ForChild]
    public NoDocumentationConfig NoDocumentation { get; }
        = new NoDocumentationConfig();

    /// <summary>
    /// Gets the configuration of ThoughtlessName analyzer.
    /// </summary>
    [field: ForChild]
    public ThoughtlessNameConfig ThoughtlessName { get; }
        = new ThoughtlessNameConfig();

    /// <inheritdoc/>
    public override IEnumerable<WhereWhy> Validate()
    {
        return Enumerables.Of<AbstractConfig>(
                ByteOrderMark,
                DiscardingReturnValue,
                LongLine,
                NoDocumentation,
                ThoughtlessName)
            .SelectMany(c => c.Validate());
    }
}
