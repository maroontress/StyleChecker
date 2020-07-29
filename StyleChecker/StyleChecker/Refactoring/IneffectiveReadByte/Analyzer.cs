namespace StyleChecker.Refactoring.IneffectiveReadByte
{
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
            var model = context.SemanticModel;
            var root = model.SyntaxTree.GetCompilationUnitRoot(
                context.CancellationToken);
            var className = typeof(BinaryReader).FullName;
            var methodName = nameof(BinaryReader.ReadByte);
            var elementType = typeof(byte[]).FullName;

            var simpleAssignments = root.DescendantNodes()
                .Where(n => n.IsKind(SyntaxKind.SimpleAssignmentExpression))
                .OfType<AssignmentExpressionSyntax>();
            foreach (var expr in simpleAssignments)
            {
                var arrayAccess = ExpressionStatements.AccessArrayElement(
                    model, expr.Left, elementType);
                if (arrayAccess is null)
                {
                    continue;
                }
                var instance = ExpressionStatements.InvocationWithNoArgument(
                    model, expr.Right, className, methodName);
                if (instance is null)
                {
                    continue;
                }
                if (!(expr.Parent is StatementSyntax statement))
                {
                    continue;
                }
                var parent = statement.Parent;
                while (parent is BlockSyntax block)
                {
                    if (block.Statements.Count > 1)
                    {
                        continue;
                    }
                    parent = block.Parent;
                }
                if (!(parent is ForStatementSyntax forStatement))
                {
                    continue;
                }
                var p = ForStatements.GetLoopIndexRange(
                    model, forStatement);
                if (p is null)
                {
                    continue;
                }
                var indexSymbol = p.Symbol;
                if (!indexSymbol.Equals(arrayAccess.Index))
                {
                    continue;
                }
                var start = p.Start;
                var end = p.End;
                var arrayName = arrayAccess.Array.Name;
                var instanceName = instance.Name;
                var location = forStatement.GetLocation();
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
    }
}
