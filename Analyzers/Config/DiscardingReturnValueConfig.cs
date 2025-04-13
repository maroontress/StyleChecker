namespace StyleChecker.Analyzers.Config;

using System.Collections.Generic;
using System.Linq;
using Maroontress.Oxbind;
using Maroontress.Roastery;

/// <summary>
/// The configuration data of DiscardingReturnValue analyzer.
/// </summary>
[ForElement("DiscardingReturnValue", Namespace)]
public sealed class DiscardingReturnValueConfig : AbstractConfig
{
#pragma warning disable IDE0052 // Remove unread private members
    [ElementSchema]
    private static readonly Schema TheSchema = Schema.Of(
        Multiple.Of<Method>());
#pragma warning restore IDE0052 // Remove unread private members

    [field: ForChild]
    private IEnumerable<Method> MethodElements { get; } = [];

    /// <summary>
    /// Gets the signatures of the methods whose return value must not be
    /// discarded.
    /// </summary>
    /// <returns>
    /// The signatures of the methods.
    /// </returns>
    public IEnumerable<string> GetMethodSignatures()
    {
        return MethodElements.Select(e => e.Id)
            .FilterNonNullReference();
    }

    /// <inheritdoc/>
    public override IEnumerable<WhereWhy> Validate() => ParseKit.NoError;

    /// <summary>
    /// Represents the method whose return value must not be discarded.
    /// </summary>
    [ForElement("method", Namespace)]
    private sealed class Method
    {
        /// <summary>
        /// Gets the signature of the method.
        /// </summary>
        [field: ForAttribute("id")]
        public string? Id { get; }
    }
}
