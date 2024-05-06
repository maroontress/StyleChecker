namespace StyleChecker.Refactoring.TypeClassParameter;

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

        bool IsNonStaticTypeofArgument(ImmutableArray<IArgumentOperation> a)
            => i < a.Length && IsNonStaticTypeofOperation(a[i].Value);

        return invocations.All(o => IsNonStaticTypeofArgument(o.Arguments));
    }

    private static void AnalyzeGlobal(
        CompilationAnalysisContext context, MethodInvocationBank global)
    {
        static Func<IMethodSymbol, IEnumerable<Call>>
            NewCallsSupplier(IEnumerable<IInvocationOperation> all)
        {
            return m =>
            {
                var invocations = all.Where(o => IsTargetMethod(o, m));
                return !invocations.Any()
                    ? []
                    : [new Call(m, invocations, R.Method)];
            };
        }

        var toCalls = NewCallsSupplier(global.GetAllOperations());
        var all = global.GetAllSymbols()
            .SelectMany(toCalls)
            .SelectMany(c => c.ToDiagnostics())
            .ToList();
        foreach (var d in all)
        {
            context.ReportDiagnostic(d);
        }
    }

    private static (IEnumerable<T> Private, IEnumerable<T> NonPrivates)
        Split<T>(IEnumerable<IGrouping<bool, T>> groups)
    {
        var map = new Dictionary<bool, IEnumerable<T>>()
        {
            [false] = [],
            [true] = [],
        };
        foreach (var g in groups)
        {
            map[g.Key] = g;
        }
        return (map[true], map[false]);
    }

    private static void AnalyzeModel(
        SemanticModelAnalysisContext context,
        MethodInvocationBank global)
    {
        static bool HasTypeClassParameter(IMethodSymbol m)
            => TypeClassParameters(m).Any();

        static bool IsPrivate(IMethodSymbol m)
            => m.DeclaredAccessibility is Accessibility.Private;

        static bool IsTargetMethodPrivate(IInvocationOperation o)
            => IsPrivate(o.TargetMethod);

        static Func<IMethodSymbol, IEnumerable<Call>> NewCallsSupplier<T>(
                string resource, Func<SyntaxNode, IOperation?> toOperation)
            where T : ISymbol
        {
            return m =>
            {
                if (m.ContainingSymbol is not T s
                    || s.DeclaringSyntaxReferences
                        .FirstOrDefault() is not {} node)
                {
                    return [];
                }
                var list = node.GetSyntax()
                    .DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Select(toOperation)
                    .OfType<IInvocationOperation>()
                    .Where(o => IsTargetMethod(o, m))
                    .ToList();
                return (!list.Any())
                    ? []
                    : [new Call(m, list, resource)];
            };
        }

        var toOperation = context.GetOperationSupplier();
        var symbolizer = context.GetSymbolizer();

        var root = context.GetCompilationUnitRoot();
        var allNodes = root.DescendantNodes();
        var localFunctions = allNodes.OfType<LocalFunctionStatementSyntax>()
            .Select(toOperation)
            .OfType<ILocalFunctionOperation>()
            .Select(o => o.Symbol)
            .Where(HasTypeClassParameter);

        var methodGroups = allNodes.OfType<MethodDeclarationSyntax>()
            .Select(symbolizer.ToSymbol)
            .FilterNonNullReference()
            .Where(m => !m.IsAbstract
                && !m.IsExtern
                && m.PartialDefinitionPart is null
                && m.PartialImplementationPart is null
                && m.ContainingType.TypeKind is not TypeKind.Interface)
            .Where(HasTypeClassParameter)
            .GroupBy(IsPrivate);
        var (privateMethods, unitMethods) = Split(methodGroups);
        global.AddSymbols(unitMethods);

        var invovationGroups = allNodes.OfType<InvocationExpressionSyntax>()
            .Select(toOperation)
            .OfType<IInvocationOperation>()
            .Where(o => o.TargetMethod.MethodKind is MethodKind.Ordinary)
            .GroupBy(IsTargetMethodPrivate);
        var (privateInvocations, unitInvocations) = Split(invovationGroups);
        global.AddOperations(unitInvocations);

        var toLocalFunctionCalls = NewCallsSupplier<IMethodSymbol>(
            R.LocalFunction, toOperation);
        var toPrivateMethodCalls = NewCallsSupplier<INamedTypeSymbol>(
            R.Method, toOperation);
        var all = localFunctions.SelectMany(toLocalFunctionCalls)
            .Concat(privateMethods.SelectMany(toPrivateMethodCalls))
            .SelectMany(i => i.ToDiagnostics())
            .ToList();
        foreach (var d in all)
        {
            context.ReportDiagnostic(d);
        }
    }

    private static bool IsTargetMethod(IInvocationOperation o, IMethodSymbol m)
    {
        return o.TargetMethod.OriginalDefinition is {} d
            && Symbols.AreEqual(d, m);
    }

    private void StartAction(CompilationStartAnalysisContext context)
    {
        var global = new MethodInvocationBank();
        context.RegisterSemanticModelAction(c => AnalyzeModel(c, global));
        context.RegisterCompilationEndAction(c => AnalyzeGlobal(c, global));
    }

    /// <summary>
    /// Represents a local function or a method, which has any paramerers of
    /// type <c>System.Type</c>, and invocation operations to call it.
    /// </summary>
    /// <param name="symbol">
    /// The symbol of the local function or the method.
    /// </param>
    /// <param name="calls">
    /// The invocation operations to call the local function or method that
    /// <paramref name="symbol"/> represents.
    /// </param>
    /// <param name="resource">
    /// The resource that represents a local function or method.
    /// </param>
    public sealed class Call(
        IMethodSymbol symbol,
        IEnumerable<IInvocationOperation> calls,
        string resource)
    {
        /// <summary>
        /// Gets the symbol of the local function or the method.
        /// </summary>
        private IMethodSymbol Symbol { get; } = symbol;

        /// <summary>
        /// Gets the resource that represents a local function or method, which
        /// is <c>R.LocalFunction</c> or <c>R.Method</c>.
        /// </summary>
        private string Resource { get; } = resource;

        /// <summary>
        /// Gets all the invocation operations to call this local function or
        /// method.
        /// </summary>
        private IEnumerable<IInvocationOperation> Callers { get; } = calls;

        /// <summary>
        /// Gets a sequence of diagnostics for the method or local function
        /// represented by this instance.
        /// </summary>
        /// <returns>
        /// An <c>IEnumerable</c> of Diagnostics. Each Diagnostic corresponds
        /// to a parameter of type <c>System.Type</c> in the method or local
        /// function, where every argument in the invocation operations is a
        /// <c>typeof</c> operator.
        /// </returns>
        public IEnumerable<Diagnostic> ToDiagnostics()
        {
            var m = Symbol;
            var parameters = m.Parameters;
            var invocations = Callers;
            return TypeClassParameters(m)
                .Select(p => p.Ordinal)
                .Where(i => IsEveryArgumentTypeofOperator(invocations, i))
                .Select(i => parameters[i])
                .SelectMany(ToTargetOrEmpty)
                .Select(t => t.ToDiagnostic(Rule));
        }

        private IEnumerable<Target> ToTargetOrEmpty(IParameterSymbol p)
        {
            return p.Locations.FirstOrDefault() is {} location
                ? [new Target(location, p.Name, Symbol.Name, Resource)]
                : [];
        }
    }

    /// <summary>
    /// Represents a target location where a diagnostic is reported.
    /// </summary>
    public sealed class Target(
        Location where, string name, string calleeName, string calleeType)
    {
        /// <summary>
        /// Gets the location where the diagnostic is reported.
        /// </summary>
        private Location Where { get; } = where;

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        private string ParameterName { get; } = name;

        /// <summary>
        /// Gets the name of the local function or the method.
        /// </summary>
        private string CalleeName { get; } = calleeName;

        /// <summary>
        /// Gets the display name of the local function or the method.
        /// </summary>
        private string CalleeType { get; } = calleeType;

        /// <summary>
        /// Gets a new diagnostic for this object.
        /// </summary>
        /// <param name="rule">
        /// The diagnostic descriptor.
        /// </param>
        /// <returns>
        /// A <see cref="Diagnostic"/> object.
        /// </returns>
        public Diagnostic ToDiagnostic(DiagnosticDescriptor rule)
        {
            return Diagnostic.Create(
                rule, Where, ParameterName, CalleeType, CalleeName);
        }
    }
}
