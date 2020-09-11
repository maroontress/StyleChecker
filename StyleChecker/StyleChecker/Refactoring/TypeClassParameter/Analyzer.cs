namespace StyleChecker.Refactoring.TypeClassParameter
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Maroontress.Extensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Operations;
    using R = Resources;

    /// <summary>
    /// TypeClassParameter analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : AbstractAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "TypeClassParameter";

        private const string Category = Categories.Refactoring;
        private static readonly DiagnosticDescriptor Rule = NewRule();

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        private protected override void Register(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(StartAction);
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

        private static bool IsOfType<T>(ITypeSymbol s)
            => s.ToString() == typeof(T).FullName;

        private static IEnumerable<IParameterSymbol>
            TypeClassParameters(IMethodSymbol m)
                => m.Parameters.Where(p => IsOfType<Type>(p.Type)
                    && p.RefKind == RefKind.None
                    && !p.IsOptional);

        private static bool IsEveryArgumentTypeofOperator(
             IEnumerable<IInvocationOperation> invocations, int i)
        {
            static bool IsNonStaticTypeofOperation(IOperation o)
                => o is ITypeOfOperation typeOfOperation
                    && !typeOfOperation.TypeOperand.IsStatic;

            bool IsNonStaticTypeofArgument(
                ImmutableArray<IArgumentOperation> a)
                => i < a.Length
                    && IsNonStaticTypeofOperation(a[i].Value);

            return invocations
                .All(o => IsNonStaticTypeofArgument(o.Arguments));
        }

        private static void AnalyzeGlobal(
            CompilationAnalysisContext context,
            List<IMethodSymbol> globalMethods,
            List<IInvocationOperation> globalInvocations)
        {
            static ImmutableArray<T> ToImmutableArray<T>(IEnumerable<T> a)
            {
                lock (a)
                {
                    return a.ToImmutableArray();
                }
            }

            var allMethods = ToImmutableArray(globalMethods);
            var allInvocations = ToImmutableArray(globalInvocations);

            foreach (var m in allMethods)
            {
                var invocations = allInvocations
                    .Where(o => IsTargetMethod(o, m));
                if (!invocations.Any())
                {
                    continue;
                }

                bool IsTypeofArgumentForAll(int i)
                    => IsEveryArgumentTypeofOperator(invocations, i);

                var all = TypeClassParameters(m)
                    .Select(p => p.Ordinal)
                    .Where(IsTypeofArgumentForAll);
                foreach (var index in all)
                {
                    var parameter = m.Parameters[index];
                    var location = parameter.Locations.FirstOrDefault();
                    if (location is null)
                    {
                        continue;
                    }
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        location,
                        parameter.Name,
                        R.Method,
                        m.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static (IEnumerable<T> Private, IEnumerable<T> NonPrivates)
            Split<T>(IEnumerable<IGrouping<bool, T>> groups)
        {
            var empty = Enumerable.Empty<T>();
            var map = new Dictionary<bool, IEnumerable<T>>()
            {
                [false] = empty,
                [true] = empty,
            };
            foreach (var g in groups)
            {
                map[g.Key] = g;
            }
            return (map[true], map[false]);
        }

        private static void AnalyzeModel(
            SemanticModelAnalysisContext context,
            List<IMethodSymbol> globalMethods,
            List<IInvocationOperation> globalInvocations)
        {
            static bool HasTypeClassParameter(IMethodSymbol m)
                => TypeClassParameters(m).Any();

            static bool IsPrivate(IMethodSymbol m)
                => m.DeclaredAccessibility is Accessibility.Private;

            static bool IsTargetMethodPrivate(IInvocationOperation o)
                => IsPrivate(o.TargetMethod);

            var operationSupplier = context.GetOperationSupplier();
            var symbolizer = context.GetSymbolizer();

            var root = context.GetCompilationUnitRoot();
            var allNodes = root.DescendantNodes();
            var localFunctions = allNodes
                .OfType<LocalFunctionStatementSyntax>()
                .Select(operationSupplier)
                .OfType<ILocalFunctionOperation>()
                .Select(o => o.Symbol)
                .Where(m => HasTypeClassParameter(m));

            var methodGroups = allNodes
                .OfType<MethodDeclarationSyntax>()
                .Select(symbolizer.ToSymbol)
                .FilterNonNullReference()
                .Where(m => !m.IsAbstract
                    && !m.IsExtern
                    && m.PartialDefinitionPart is null
                    && m.PartialImplementationPart is null
                    && !(m.ContainingType.TypeKind is TypeKind.Interface))
                .Where(HasTypeClassParameter)
                .GroupBy(IsPrivate);
            var (privateMethods, unitMethods) = Split(methodGroups);
            lock (globalMethods)
            {
                globalMethods.AddRange(unitMethods);
            }

            var invovationGroups = allNodes
                .OfType<InvocationExpressionSyntax>()
                .Select(operationSupplier)
                .OfType<IInvocationOperation>()
                .Where(o => o.TargetMethod.MethodKind is MethodKind.Ordinary)
                .GroupBy(IsTargetMethodPrivate);
            var (privateInvocations, unitInvocations)
                = Split(invovationGroups);
            lock (globalInvocations)
            {
                globalInvocations.AddRange(unitInvocations);
            }

            static (IMethodSymbol Symbol,
                ISymbol? ContainingSymbol,
                string Format) ToLocalFunctionTuple(IMethodSymbol m)
            {
                return (m,
                    m.ContainingSymbol as IMethodSymbol,
                    R.LocalFunction);
            }

            static (IMethodSymbol Symbol,
                ISymbol? ContainingSymbol,
                string Format) ToPrivateMethodTuple(IMethodSymbol m)
            {
                return (m,
                    m.ContainingSymbol as INamedTypeSymbol,
                    R.Method);
            }

            var list = localFunctions.Select(ToLocalFunctionTuple)
                .Concat(privateMethods.Select(ToPrivateMethodTuple));
            foreach (var (m, containingSymbol, format) in list)
            {
                if (containingSymbol is null)
                {
                    continue;
                }
                var node = containingSymbol.DeclaringSyntaxReferences
                    .FirstOrDefault();
                if (node is null)
                {
                    continue;
                }
                var invocations = node.GetSyntax().DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Select(operationSupplier)
                    .OfType<IInvocationOperation>()
                    .Where(o => IsTargetMethod(o, m));
                if (!invocations.Any())
                {
                    continue;
                }

                bool IsTypeofArgumentForAll(int i)
                    => IsEveryArgumentTypeofOperator(invocations, i);

                var all = TypeClassParameters(m)
                    .Select(p => p.Ordinal)
                    .Where(IsTypeofArgumentForAll);
                foreach (var index in all)
                {
                    var parameter = m.Parameters[index];
                    var location = parameter.Locations.FirstOrDefault();
                    if (location is null)
                    {
                        continue;
                    }
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        location,
                        parameter.Name,
                        format,
                        m.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool IsTargetMethod(
            IInvocationOperation o, IMethodSymbol m)
        {
            var d = o.TargetMethod.OriginalDefinition;
            return !(d is null) && Symbols.AreEqual(d, m);
            /*
                return o.TargetMethod.OriginalDefinition is {} d
                    && Symbols.AreEqual(d, m);
            */
        }

        private void StartAction(CompilationStartAnalysisContext context)
        {
            var globalMethods = new List<IMethodSymbol>();
            var globalInvocations = new List<IInvocationOperation>();
            context.RegisterSemanticModelAction(
                c => AnalyzeModel(c, globalMethods, globalInvocations));
            context.RegisterCompilationEndAction(
                c => AnalyzeGlobal(c, globalMethods, globalInvocations));
        }
    }
}
