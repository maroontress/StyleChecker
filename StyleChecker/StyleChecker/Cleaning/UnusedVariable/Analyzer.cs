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
    using StyleChecker.Annotations;
    using StyleChecker.Invocables;
    using StyleChecker.Naming;
    using Enumerables = Maroontress.Util.Enumerables;
    using R = Resources;

    /// <summary>
    /// UnusedVariable analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : AbstractAnalyzer
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
            IEnumerable<ILocalSymbol> FindLocalSymbols(SyntaxToken token)
            {
                var first = model.LookupSymbols(
                        token.Span.Start, null, token.ValueText)
                    .OfType<ILocalSymbol>()
                    .FirstOrDefault();
                return (!(first is null))
                    ? Enumerables.Of(first)
                    : Enumerable.Empty<ILocalSymbol>();
            }

            var all = LocalVariables.Symbols(model)
                .ToArray();
            if (!all.Any())
            {
                return;
            }
            foreach (var (token, symbol) in all)
            {
                var containingSymbol = symbol.ContainingSymbol;
                var reference = containingSymbol.DeclaringSyntaxReferences
                    .FirstOrDefault();
                if (reference is null)
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

            static IEnumerable<InvocableBaseNodePod> ToPods(SyntaxNode n)
            {
                var p = InvocableBaseNodePod.Of(n);
                return (p is null)
                    ? Enumerable.Empty<InvocableBaseNodePod>()
                    : Enumerables.Of(p);
            }

            static bool IsEmptyBody(InvocableBaseNodePod pod)
            {
                return (pod.Body is null || !pod.Body.ChildNodes().Any())
                    && pod.ExpressionBody is null;
            }

            static bool IsMarkedAsUnused(AttributeData d)
            {
                var clazz = d.AttributeClass;
                return !(clazz is null)
                    && clazz.ToString() == typeof(UnusedAttribute).FullName;
            }

            static bool ShouldParametersBeUsed(
                (InvocableBaseNodePod Pod, IMethodSymbol Symbol) i)
            {
                var (p, m) = i;
                return !m.IsAbstract
                    && !((m.IsExtern
                            || m.IsVirtual
                            || p.Modifiers.Any(o => o.Text is "partial"))
                        && IsEmptyBody(p));
            }

            static bool IsParameterMarkedAsUnused(IParameterSymbol p)
            {
                return p.GetAttributes()
                    .Any(IsMarkedAsUnused);
            }

            void Report(IParameterSymbol p, string m)
            {
                var reference = p.DeclaringSyntaxReferences
                    .FirstOrDefault();
                if (reference is null
                    || !(reference.GetSyntax() is ParameterSyntax node))
                {
                    return;
                }
                var token = node.Identifier;
                var location = p.Locations[0];
                var diagnostic = Diagnostic.Create(
                    Rule, location, R.TheParameter, token, m);
                context.ReportDiagnostic(diagnostic);
            }

            void ReportIfUnnecessarilyMarked(IMethodSymbol m)
            {
                var all = m.Parameters
                    .Where(IsParameterMarkedAsUnused);
                foreach (var p in all)
                {
                    Report(p, R.MarkIsUnnecessary);
                }
            }

            Action ToAction(
                IEnumerable<IParameterSymbol> g,
                string m,
                Func<bool, bool> b)
            {
                var all = g.Where(p => b(IsParameterMarkedAsUnused(p)));
                return () =>
                {
                    foreach (var q in all)
                    {
                        Report(q, m);
                    }
                };
            }

            void ReportIfUnusedOrMismarked(
                (InvocableBaseNodePod Pod, IMethodSymbol Symbol) i)
            {
                var (pod, m) = i;
                var identifierNames = pod.Node
                    .DescendantNodes()
                    .OfType<IdentifierNameSyntax>();
                var all = m.Parameters
                    .GroupBy(
                        p => identifierNames.Any(
                            n => FindSymbols(model, n.Identifier)
                                .Any(s => s.Equals(p))),
                        (isUsed, group) => isUsed
                            ? ToAction(group, R.UsedButMarkedAsUnused, b => b)
                            : ToAction(group, R.NeverUsed, b => !b));
                foreach (var action in all)
                {
                    action();
                }
            }

            IEnumerable<(InvocableBaseNodePod Pod, IMethodSymbol Symbol)>
                ToInvocation(InvocableBaseNodePod p)
            {
                var s = model.GetDeclaredSymbol(p.Node, cancellationToken);
                return (s is IMethodSymbol m)
                    ? Enumerables.Of((p, m))
                    : Enumerable.Empty<(InvocableBaseNodePod,
                        IMethodSymbol)>();
            }

            var methods = root.DescendantNodes()
                .OfType<BaseMethodDeclarationSyntax>();
            var localFunctions = root.DescendantNodes()
                .OfType<LocalFunctionStatementSyntax>();
            var invocations = Enumerable.Empty<SyntaxNode>()
                .Concat(methods)
                .Concat(localFunctions)
                .SelectMany(ToPods)
                .SelectMany(ToInvocation)
                .ToArray();
            if (!invocations.Any())
            {
                return;
            }
            foreach (var i in invocations)
            {
                if (ShouldParametersBeUsed(i))
                {
                    ReportIfUnusedOrMismarked(i);
                }
                else
                {
                    ReportIfUnnecessarilyMarked(i.Symbol);
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
            return (!(first is null))
                ? new[] { first } : Array.Empty<ISymbol>();
        }
    }
}
