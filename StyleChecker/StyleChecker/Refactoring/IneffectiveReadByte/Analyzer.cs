namespace StyleChecker.Refactoring.IneffectiveReadByte
{
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using R = Resources;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "IneffectiveReadByte";

        private const string Category = Categories.Refactoring;
        private static readonly DiagnosticDescriptor Rule;

        static Analyzer()
        {
            var localize = Localizers.Of(R.ResourceManager, typeof(R));
            Rule = new DiagnosticDescriptor(
                DiagnosticId,
                localize(nameof(R.Title)),
                localize(nameof(R.MessageFormat)),
                Category,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: localize(nameof(R.Description)));
        }

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSemanticModelAction(AnalyzeModel);
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
                .Where(n => n.IsKind(SyntaxKind.SimpleAssignmentExpression));
            foreach (var s in simpleAssignments)
            {
                var expr = s as AssignmentExpressionSyntax;
                var arrayAccess = ExpressionStatements.AccessArrayElement(
                    model, expr.Left, elementType);
                if (arrayAccess == null)
                {
                    continue;
                }
                var instance = ExpressionStatements.InvocationWithNoArgument(
                    model, expr.Right, className, methodName);
                if (instance == null)
                {
                    continue;
                }
                var statement = s.Parent as StatementSyntax;
                if (statement == null)
                {
                    continue;
                }
                var parent = statement.Parent;
                while (parent is BlockSyntax block)
                {
                    if (block.Statements.Count() > 1)
                    {
                        continue;
                    }
                    parent = block.Parent;
                }
                var forStatement = parent as ForStatementSyntax;
                if (forStatement == null)
                {
                    continue;
                }
                var p = ForStatements.GetLoopIndexRange(
                    model, forStatement);
                if (p == null)
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
                var array = arrayAccess.Array;
                var location = expr.GetLocation();
                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    instance.Name,
                    array.Name,
                    start,
                    end - start + 1);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
