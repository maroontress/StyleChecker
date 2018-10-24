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

        /// <summary>
        /// The function that takes the fully qualified name of the method
        /// (including its signature) and returns whether the return value of
        /// it is ignorable or not; <c>true</c> if it is not ignorable,
        /// <c>false</c> otherwise.
        /// </summary>
        public static readonly Func<string, bool> UnignorableReturnValue;

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

            var methodSet = new HashSet<string>()
            {
                "System.IO.Stream.Read(byte[], int, int)",
                "System.IO.BinaryReader.Read(byte[], int, int)",
            };
            UnignorableReturnValue = name => methodSet.Contains(name);
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
            var cancellationToken = context.CancellationToken;
            var model = context.SemanticModel;
            var root = model.SyntaxTree
                .GetCompilationUnitRoot(cancellationToken);
            var all = root.DescendantNodes()
                .OfType<ExpressionStatementSyntax>()
                .Select(s => s.Expression)
                .OfType<InvocationExpressionSyntax>()
                .ToList();
            if (all.Count() == 0)
            {
                return;
            }
            foreach (var invocationExpr in all)
            {
                var operation = model.GetOperation(invocationExpr);
                if (!(operation is IInvocationOperation invocationOperation))
                {
                    continue;
                }
                var target = invocationOperation.TargetMethod;
                var targetName = target.ToString();
                if (!UnignorableReturnValue(targetName))
                {
                    continue;
                }

                var location = invocationExpr.Parent.GetLocation();
                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    targetName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
