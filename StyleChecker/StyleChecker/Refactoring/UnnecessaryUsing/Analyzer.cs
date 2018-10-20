namespace StyleChecker.Refactoring.UnnecessaryUsing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using R = Resources;

    /// <summary>
    /// UnnecessaryUsing analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "UnnecessaryUsing";

        /// <summary>
        /// The function that takes a class name and returns whether the class
        /// has any resources to dispose or not; <c>true</c> if it disposes
        /// nothing, <c>false</c> otherwise.
        /// </summary>
        public static readonly Func<string, bool> DisposesNothing;

        private const string Category = Categories.Refactoring;
        private static readonly DiagnosticDescriptor Rule;
        private static readonly IEnumerable<ISymbol> EmptySymbol
            = Array.Empty<ISymbol>();

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

            var classSet = new HashSet<string>()
            {
                "System.IO.MemoryStream",
                "System.IO.UnmanagedMemoryStream",
            };
            DisposesNothing = name => classSet.Contains(name);
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
                .OfType<UsingStatementSyntax>()
                .ToList();
            if (all.Count() == 0)
            {
                return;
            }
            IEnumerable<ISymbol> ToSymbols(
                VariableDeclaratorSyntax v, Func<string, bool> matches)
            {
                var symbol = GetSymbol(model, v.Identifier);
                var value = v.Initializer.Value;
                var operation = model.GetOperation(value, cancellationToken);
                var typeSymbol = operation.Type;
                return matches(TypeSymbols.GetFullName(typeSymbol))
                    ? Singleton(symbol) : EmptySymbol;
            }
            foreach (var @using in all)
            {
                var declaration = @using.Declaration;
                if (declaration == null)
                {
                    continue;
                }
                var first = declaration.Variables
                    .SelectMany(v => ToSymbols(v, DisposesNothing))
                    .FirstOrDefault();
                if (first == null)
                {
                    continue;
                }

                var location = @using.GetLocation();
                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    first.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static IEnumerable<T> Singleton<T>(T element)
            => new[] { element };

        private static ISymbol GetSymbol(
            SemanticModel model, SyntaxToken token)
        {
            var span = token.Span;
            return model.LookupSymbols(span.Start, null, token.Text)
                .First();
        }
    }
}
