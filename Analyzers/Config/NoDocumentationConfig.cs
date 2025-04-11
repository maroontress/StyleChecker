namespace Analyzers.Config;

using System.Collections.Generic;
using System.Linq;
using Analyzers.Document.NoDocumentation;
using CodeDebt.Extensions;
using Maroontress.Oxbind;

/// <summary>
/// The configuration data of NoDocumentation analyzer.
/// </summary>
[ForElement(Analyzer.DiagnosticId, Namespace)]
public sealed class NoDocumentationConfig : AbstractConfig
{
#pragma warning disable IDE0052 // Remove unread private members
    [ElementSchema]
    private static readonly Schema TheSchema = Schema.Of(
        Multiple.Of<Ignore>());
#pragma warning restore IDE0052 // Remove unread private members

    [field: ForChild]
    private IEnumerable<Ignore> IgnoreElements { get; } = [];

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
    private sealed class Ignore : Validateable
    {
        /// <summary>
        /// Gets the attribute class.
        /// </summary>
        [field: ForAttribute("with")]
        public string? With { get; }

        /// <summary>
        /// Gets whether the element only is ignored or all the elements it
        /// contains are ignored.
        /// </summary>
        [field: ForAttribute("inclusive")]
        public BindEvent<string>? InclusiveEvent { get; }

        /// <inheritdoc/>
        public IEnumerable<WhereWhy> Validate()
            => ParseKit.ValidateBoolean(
                InclusiveEvent,
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
            return ParseKit.ToBooleanValue(InclusiveEvent, false);
        }
    }
}
