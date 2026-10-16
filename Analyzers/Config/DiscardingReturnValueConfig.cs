namespace StyleChecker.Analyzers.Config;

using System.Collections.Generic;
using System.Linq;
using Maroontress.Oxbind;
using Maroontress.Roastery;

/// <summary>
/// The configuration data of DiscardingReturnValue analyzer.
/// </summary>
/// <param name="methodElements">
/// The collection of &lt;method&gt; elements from configuration. Each element
/// represents a method whose return value must not be discarded.
/// </param>
[ForElement("DiscardingReturnValue", Namespace)]
public sealed class DiscardingReturnValueConfig(
    [Multiple] IEnumerable<DiscardingReturnValueConfig.Method> methodElements)
    : AbstractConfig
{
    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="DiscardingReturnValueConfig"/> class.
    /// </summary>
    [Ignored]
    public DiscardingReturnValueConfig()
        : this([])
    {
    }

    private IEnumerable<Method> MethodElements { get; } = methodElements;

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
    /// <param name="id">
    /// The signature of the method.
    /// </param>
    [ForElement("method", Namespace)]
    public sealed class Method([ForAttribute("id")] string? id)
    {
        /// <summary>
        /// Gets the signature of the method.
        /// </summary>
        public string? Id { get; } = id;
    }
}
