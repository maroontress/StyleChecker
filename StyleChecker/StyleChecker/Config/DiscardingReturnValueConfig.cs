#pragma warning disable CA1812, CA1823

namespace StyleChecker.Config;

using System.Collections.Generic;
using System.Linq;
using Maroontress.Extensions;
using Maroontress.Oxbind;

/// <summary>
/// The configuration data of DiscardingReturnValue analyzer.
/// </summary>
[ForElement("DiscardingReturnValue", Namespace)]
public sealed class DiscardingReturnValueConfig : AbstractConfig
{
    [ElementSchema]
    private static readonly Schema TheSchema = Schema.Of(
        Multiple.Of<Method>());

    [field: ForChild]
    private IEnumerable<Method> MethodElements { get; }
        = Enumerable.Empty<Method>();

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
