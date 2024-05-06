namespace StyleChecker.Refactoring.IneffectiveReadByte;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using R = Resources;

/// <summary>
/// IneffectiveReadByte analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "IneffectiveReadByte";

    private const string Category = Categories.Refactoring;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.RegisterSemanticModelAction(AnalyzeModel);
    }

    private static DiagnosticDescriptor NewRule()
    {
        var localize = Localizers.Of<R>(R.ResourceManager);
        return new DiagnosticDescriptor(
            DiagnosticId,
            localize(nameof(R.Title)),
            localize(nameof(R.MessageFormat)),
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: localize(nameof(R.Description)),
            helpLinkUri: HelpLink.ToUri(DiagnosticId));
    }

    private static void AnalyzeModel(
        SemanticModelAnalysisContext context)
    {
        static SyntaxNode? GetFirstAncestorOtherThanBlock(SyntaxNode s)
        {
            var parent = s.Parent;
            while (parent is BlockSyntax block)
            {
                if (block.Statements.Count > 1)
                {
                    return null;
                }
                parent = block.Parent;
            }
            return parent;
        }

        var model = context.SemanticModel;
        var root = model.SyntaxTree.GetCompilationUnitRoot(
            context.CancellationToken);
        var className = typeof(BinaryReader).FullName;
        var methodName = nameof(BinaryReader.ReadByte);
        var elementType = typeof(byte[]).FullName;

        IEnumerable<BadForStatement> ToBadForStatements(
            AssignmentExpressionSyntax s)
        {
            return (ExpressionStatements.AccessArrayElement(
                    model, s.Left, elementType) is not {} arrayAccess
                    || ExpressionStatements.InvocationWithNoArgument(
                        model, s.Right, className, methodName)
                        is not {} instance
                    || s.Parent is not StatementSyntax statement
                    || GetFirstAncestorOtherThanBlock(statement)
                        is not ForStatementSyntax forStatement)
                    || ForStatements.GetLoopIndexRange(model, forStatement)
                        is not {} loopIndexRange
                    || !Symbols.AreEqual(
                        loopIndexRange.Symbol, arrayAccess.Index)
                ? []
                : [new BadForStatement(
                    arrayAccess, instance, forStatement, loopIndexRange)];
        }

        var all = root.DescendantNodes()
            .Where(n => n.IsKind(SyntaxKind.SimpleAssignmentExpression))
            .OfType<AssignmentExpressionSyntax>()
            .SelectMany(ToBadForStatements)
            .ToList();
        foreach (var s in all)
        {
            var range = s.LoopIndexRange;
            var start = range.Start;
            var end = range.End;
            var arrayName = s.ArrayAccess.Array.Name;
            var instanceName = s.Instance.Name;
            var location = s.ForStatement.GetLocation();
            var culture = CultureInfo.InvariantCulture;
            var properties = new Dictionary<string, string?>()
            {
                ["offset"] = start.ToString(culture),
                ["length"] = (end - start + 1).ToString(culture),
                ["array"] = arrayName,
                ["instance"] = instanceName,
            }.ToImmutableDictionary();
            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                properties,
                instanceName,
                arrayName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private sealed class BadForStatement(
        ArrayAccess arrayAccess,
        ISymbol instance,
        ForStatementSyntax @for,
        LoopIndexRange loopIndexRange)
    {
        public ArrayAccess ArrayAccess { get; } = arrayAccess;

        public ISymbol Instance { get; } = instance;

        public ForStatementSyntax ForStatement { get; } = @for;

        public LoopIndexRange LoopIndexRange { get; } = loopIndexRange;
    }
}
