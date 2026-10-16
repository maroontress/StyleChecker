namespace StyleChecker.Analyzers.Config;

using System.Collections.Generic;
using System.Linq;
using Maroontress.Oxbind;

/// <summary>
/// The root configuration.
/// </summary>
/// <param name="byteOrderMark">
/// Configuration for the Byte Order Mark analyzer.
/// </param>
/// <param name="discardingReturnValue">
/// Configuration for the Discarding Return Value analyzer.
/// </param>
/// <param name="longLine">
/// Configuration for the Long Line analyzer.
/// </param>
/// <param name="noDocumentation">
/// Configuration for the No Documentation analyzer.
/// </param>
/// <param name="thoughtlessName">
/// Configuration for the Thoughtless Name analyzer.
/// </param>
[ForElement("config", Namespace)]
public sealed class RootConfig(
    [Optional] ByteOrderMarkConfig? byteOrderMark,
    [Optional] DiscardingReturnValueConfig? discardingReturnValue,
    [Optional] LongLineConfig? longLine,
    [Optional] NoDocumentationConfig? noDocumentation,
    [Optional] ThoughtlessNameConfig? thoughtlessName)
    : AbstractConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootConfig"/> class.
    /// </summary>
    [Ignored]
    public RootConfig()
        : this(null, null, null, null, null)
    {
    }

    /// <summary>
    /// Gets the configuration of LongLine analyzer.
    /// </summary>
    public ByteOrderMarkConfig ByteOrderMark { get; }
        = byteOrderMark ?? new();

    /// <summary>
    /// Gets the configuration of ThoughtlessName analyzer.
    /// </summary>
    public DiscardingReturnValueConfig DiscardingReturnValue { get; }
        = discardingReturnValue ?? new();

    /// <summary>
    /// Gets the configuration of LongLine analyzer.
    /// </summary>
    public LongLineConfig LongLine { get; }
        = longLine ?? new();

    /// <summary>
    /// Gets the configuration of NoDocumentation analyzer.
    /// </summary>
    public NoDocumentationConfig NoDocumentation { get; }
        = noDocumentation ?? new();

    /// <summary>
    /// Gets the configuration of ThoughtlessName analyzer.
    /// </summary>
    public ThoughtlessNameConfig ThoughtlessName { get; }
        = thoughtlessName ?? new();

    /// <inheritdoc/>
    public override IEnumerable<WhereWhy> Validate()
    {
        IEnumerable<AbstractConfig> all = [
            ByteOrderMark,
            DiscardingReturnValue,
            LongLine,
            NoDocumentation,
            ThoughtlessName,
        ];
        return all.SelectMany(c => c.Validate());
    }
}
