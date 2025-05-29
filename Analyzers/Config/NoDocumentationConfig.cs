namespace StyleChecker.Analyzers.Config;

using System.Collections.Generic;
using System.Linq;
using Maroontress.Oxbind;
using Maroontress.Roastery;
using StyleChecker.Analyzers.Document.NoDocumentation;

/// <summary>
/// The configuration data of NoDocumentation analyzer.
/// </summary>
[ForElement(Analyzer.DiagnosticId, Namespace)]
public sealed class NoDocumentationConfig(
    [Multiple] IEnumerable<NoDocumentationConfig.Ignore> ignoreElements)
    : AbstractConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoDocumentationConfig"/>
    /// class.
    /// </summary>
    [Ignored]
    public NoDocumentationConfig()
        : this([])
    {
    }

    private IEnumerable<Ignore> IgnoreElements { get; } = ignoreElements;

    /// <summary>
    /// Gets the attribute classes, with which the element annotated and
    /// the elements it contains are ignored.
    /// </summary>
    /// <returns>
    /// The attribute classes.
    /// </returns>
    public IEnumerable<string> GetInclusiveAttributes()
    {
        return IgnoreElements.Where(e => e.IsInclusive())
            .Select(e => e.With)
            .FilterNonNullReference();
    }

    /// <summary>
    /// Gets the attribute classes, with which the element annotated only
    /// is ignored.
    /// </summary>
    /// <returns>
    /// The attribute classes.
    /// </returns>
    public IEnumerable<string> GetAttributes()
    {
        return IgnoreElements.Where(e => !e.IsInclusive())
            .Select(e => e.With)
            .FilterNonNullReference();
    }

    /// <inheritdoc/>
    public override IEnumerable<WhereWhy> Validate()
    {
        return IgnoreElements.SelectMany(e => e.Validate());
    }

    /// <summary>
    /// Represents the attribute class with which the element annotated
    /// must be ignored.
    /// </summary>
    [ForElement("ignore", Namespace)]
    public sealed class Ignore(
        [ForAttribute("with")] string? with,
        [ForAttribute("inclusive")] BindResult<string>? inclusiveResult)
        : Validateable
    {
        /// <summary>
        /// Gets the attribute class.
        /// </summary>
        public string? With { get; } = with;

        /// <summary>
        /// Gets whether the element only is ignored or all the elements it
        /// contains are ignored.
        /// </summary>
        public BindResult<string>? InclusiveResult { get; } = inclusiveResult;

        /// <inheritdoc/>
        public IEnumerable<WhereWhy> Validate()
            => ParseKit.ValidateBoolean(
                InclusiveResult,
                "invalid boolean value of 'inclusive' attribute");

        /// <summary>
        /// Returns the whether the range to be ignored is only the element
        /// itself or all the elements it contains.
        /// </summary>
        /// <returns>
        /// <c>false</c> if the range to be ignored is only the element,
        /// <c>true</c> otherwise.
        /// </returns>
        public bool IsInclusive()
        {
            return ParseKit.ToBooleanValue(InclusiveResult, false);
        }
    }
}
