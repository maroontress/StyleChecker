namespace StyleChecker.Refactoring.DiscardingReturnValue
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Operations;
    using StyleChecker.Annotations;
    using R = Resources;

    /// <summary>
    /// DiscardingReturnValue analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "DiscardingReturnValue";

        private const string Category = Categories.Refactoring;

        /// <summary>
        /// The function that takes the fully qualified name of the method
        /// (including its signature) and returns whether the return value of
        /// it is ignorable or not; <c>true</c> if it is not ignorable,
        /// <c>false</c> otherwise.
        /// </summary>
        private static readonly Func<IMethodSymbol, bool>
            TargetMethodPredicate = NewTargetMethodPredicate();

        private static readonly DiagnosticDescriptor Rule = NewRule();
        private static readonly string DoNotIgnoreClassName
            = typeof(DoNotIgnoreAttribute).FullName;

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

        private static DiagnosticDescriptor NewRule()
        {
            var localize = Localizers.Of(R.ResourceManager, typeof(R));
            return new DiagnosticDescriptor(
                DiagnosticId,
                localize(nameof(R.Title)),
                localize(nameof(R.MessageFormat)),
                Category,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: localize(nameof(R.Description)));
        }

        private static Func<IMethodSymbol, bool> NewTargetMethodPredicate()
        {
            var methodNames = new HashSet<string>()
            {
                "System.IO.Stream.Read(byte[], int, int)",
                "System.IO.BinaryReader.Read(byte[], int, int)",
            };
            var typeNames = R.TypeNames.Split(
                    new[] { "\r\n" },
                    StringSplitOptions.RemoveEmptyEntries)
                .ToImmutableHashSet();
            var typePredicates
                = new Dictionary<string, Func<IMethodSymbol, bool>>
            {
                ["System.Type"] = m => !m.Name.Equals("InvokeMember"),
            };
            return m =>
            {
                if (methodNames.Contains(m.ToString()))
                {
                    return true;
                }
                var containingType = m.ContainingType.OriginalDefinition;
                var type = containingType.ToString();
                if (typeNames.Contains(type))
                {
                    return true;
                }
                var containingNamespace = m.ContainingNamespace;
                if (!typePredicates.TryGetValue(
                    $"{containingNamespace.Name}.{containingType.Name}",
                    out var predicate))
                {
                    return false;
                }
                return predicate(m);
            };
        }

        private static void AnalyzeModel(
            SemanticModelAnalysisContext context)
        {
            var cancellationToken = context.CancellationToken;
            var model = context.SemanticModel;
            var root = model.SyntaxTree
                .GetCompilationUnitRoot(cancellationToken);
            var all = root.DescendantNodes()
                .OfType<ExpressionStatementSyntax>()
                .Select(s => s.Expression)
                .OfType<InvocationExpressionSyntax>();
            if (all.Count() == 0)
            {
                return;
            }
            bool IsMarkedAsDoNotIgnore(IMethodSymbol s)
                => s.GetReturnTypeAttributes()
                    .Select(d => d.AttributeClass.ToString())
                    .Where(n => n == DoNotIgnoreClassName)
                    .Any();

            foreach (var invocationExpr in all)
            {
                var operation = model.GetOperation(invocationExpr);
                if (!(operation is IInvocationOperation invocationOperation))
                {
                    continue;
                }
                var target = invocationOperation.TargetMethod;
                if (target.ReturnsVoid)
                {
                    continue;
                }
                if (!IsMarkedAsDoNotIgnore(target)
                    && !TargetMethodPredicate(target))
                {
                    continue;
                }

                var location = invocationExpr.Parent.GetLocation();
                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    target.OriginalDefinition.ToString());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
