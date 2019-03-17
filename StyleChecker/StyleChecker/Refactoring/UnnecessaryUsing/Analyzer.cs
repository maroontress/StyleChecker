namespace StyleChecker.Refactoring.UnnecessaryUsing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Operations;
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
        public static readonly Func<string, bool> DisposesNothing
            = NewDisposesNothing();

        private const string Category = Categories.Refactoring;

        private static readonly DiagnosticDescriptor Rule = NewRule();

        private static readonly IEnumerable<ISymbol> EmptySymbol
            = Array.Empty<ISymbol>();

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

        private static Func<string, bool> NewDisposesNothing()
        {
            var classSet = new HashSet<string>()
            {
                typeof(MemoryStream).FullName,
                typeof(StringReader).FullName,
                typeof(StringWriter).FullName,
                "System.IO.UnmanagedMemoryAccessor",
                "System.IO.UnmanagedMemoryStream",
            };
            return name => classSet.Contains(name);
        }

        private static void AnalyzeModel(
            SemanticModelAnalysisContext context)
        {
            var cancellationToken = context.CancellationToken;
            var model = context.SemanticModel;
            var root = model.SyntaxTree
                .GetCompilationUnitRoot(cancellationToken);
            var all = root.DescendantNodes()
                .OfType<UsingStatementSyntax>();
            if (!all.Any())
            {
                return;
            }
            IEnumerable<ISymbol> ToSymbols(
                VariableDeclaratorSyntax v, Func<string, bool> matches)
            {
                var declaratorOperation = model.GetOperation(
                    v, cancellationToken)
                    as IVariableDeclaratorOperation;
                var symbol = declaratorOperation.Symbol;
                var value = v.Initializer.Value;
                var operation = model.GetOperation(value);
                var typeSymbol = operation.Type;
                return matches(TypeSymbols.GetFullName(typeSymbol))
                    ? Create(symbol) : EmptySymbol;
            }
            foreach (var @using in all)
            {
                var declaration = @using.Declaration;
                if (declaration is null)
                {
                    continue;
                }
                var first = declaration.Variables
                    .SelectMany(v => ToSymbols(v, DisposesNothing))
                    .FirstOrDefault();
                if (first is null)
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

        private static IEnumerable<T> Create<T>(params T[] elements)
            => elements;
    }
}
