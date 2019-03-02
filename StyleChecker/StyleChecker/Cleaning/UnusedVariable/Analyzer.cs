namespace StyleChecker.Cleaning.UnusedVariable
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Syntax;
    using StyleChecker.Annotations;
    using StyleChecker.Naming;
    using R = Resources;

    /// <summary>
    /// UnusedVariable analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "UnusedVariable";

        private const string Category = Categories.Cleaning;
        private static readonly DiagnosticDescriptor Rule = NewRule();

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

        private static void AnalyzeModel(
            SemanticModelAnalysisContext context)
        {
            var cancellationToken = context.CancellationToken;
            var model = context.SemanticModel;
            var root = model.SyntaxTree.GetCompilationUnitRoot(
                cancellationToken);
            CheckLocal(context, model);
            CheckParameter(context, model, root);
        }

        private static void CheckLocal(
            SemanticModelAnalysisContext context,
            SemanticModel model)
        {
            var all = LocalVariables.Symbols(model).ToList();
            if (all.Count == 0)
            {
                return;
            }
            ILocalSymbol[] FindLocalSymbols(SyntaxToken token)
            {
                var first = model.LookupSymbols(
                        token.Span.Start, null, token.ValueText)
                    .OfType<ILocalSymbol>()
                    .FirstOrDefault();
                return (first != null)
                    ? new[] { first } : Array.Empty<ILocalSymbol>();
            }
            foreach (var (token, symbol) in all)
            {
                var containingSymbol = symbol.ContainingSymbol;
                var reference = containingSymbol.DeclaringSyntaxReferences
                    .FirstOrDefault();
                if (reference == null)
                {
                    continue;
                }
                var node = reference.GetSyntax();
                if (node.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Any(n => FindLocalSymbols(n.Identifier)
                        .Any(s => s.Equals(symbol))))
                {
                    continue;
                }
                var diagnostic = Diagnostic.Create(
                    Rule,
                    token.GetLocation(),
                    R.TheLocalVariable,
                    token,
                    R.NeverUsed);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void CheckParameter(
            SemanticModelAnalysisContext context,
            SemanticModel model,
            CompilationUnitSyntax root)
        {
            var cancellationToken = context.CancellationToken;
            var methods = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .SelectMany(s => s.Members)
                .OfType<BaseMethodDeclarationSyntax>()
                .ToList();
            if (methods.Count == 0)
            {
                return;
            }
            bool IsEmptyBody(BaseMethodDeclarationSyntax node)
            {
                return (node.Body == null || !node.Body.ChildNodes().Any())
                    && node.ExpressionBody == null;
            }
            bool IsMarkedAsUnused(AttributeData d)
                => d.AttributeClass.ToString()
                    == typeof(UnusedAttribute).FullName;
            void Report(IParameterSymbol p, string m)
            {
                var reference = p.DeclaringSyntaxReferences.FirstOrDefault();
                if (reference == null
                    || !(reference.GetSyntax() is ParameterSyntax node))
                {
                    return;
                }
                var token = node.Identifier;
                var location = p.Locations[0];
                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    R.TheParameter,
                    token,
                    m);
                context.ReportDiagnostic(diagnostic);
            }
            foreach (var node in methods)
            {
                var m = model.GetDeclaredSymbol(node, cancellationToken);
                var parameters = m.Parameters;
                if (m.IsAbstract
                    || (m.IsExtern && IsEmptyBody(node))
                    || (node.Modifiers
                            .Any(o => o.Text.Equals("partial"))
                        && IsEmptyBody(node))
                    || (m.IsVirtual && IsEmptyBody(node)))
                {
                    foreach (var p in parameters)
                    {
                        if (p.GetAttributes()
                            .Where(IsMarkedAsUnused)
                            .Any())
                        {
                            Report(p, R.MarkIsUnnecessary);
                        }
                    }
                    continue;
                }
                var identifierNames = node.DescendantNodes()
                    .OfType<IdentifierNameSyntax>();
                foreach (var p in parameters)
                {
                    var attributes = p.GetAttributes();
                    var marksAsUnused = attributes
                        .Where(IsMarkedAsUnused)
                        .Any();
                    if (identifierNames
                        .Any(n => FindSymbols(model, n.Identifier)
                            .Any(s => s.Equals(p))))
                    {
                        if (!marksAsUnused)
                        {
                            continue;
                        }
                        Report(p, R.UsedButMarkedAsUnused);
                        continue;
                    }
                    if (marksAsUnused)
                    {
                        continue;
                    }
                    Report(p, R.NeverUsed);
                }
            }
        }

        private static IEnumerable<ISymbol> FindSymbols(
            SemanticModel model, SyntaxToken token)
        {
            var first = model.LookupSymbols(
                    token.Span.Start, null, token.ValueText)
                .OfType<ISymbol>()
                .FirstOrDefault();
            return (first != null)
                ? new[] { first } : Array.Empty<ISymbol>();
        }
    }
}
