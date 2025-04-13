namespace StyleChecker.Analyzers.Refactoring.NoUsingDeclaration;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using StyleChecker.Analyzers;
using R = Resources;

/// <summary>
/// NoUsingDeclaration analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "NoUsingDeclaration";

    private const string Category = Categories.Refactoring;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    private static Func<ISymbol, ISymbol, bool> SymbolEquals { get; }
        = SymbolEqualityComparer.Default.Equals;

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

    private static void AnalyzeModel(SemanticModelAnalysisContext context)
    {
        var token = context.CancellationToken;
        var model = context.SemanticModel;
        var toImplicitTypeDiagnostics = NewDiagnosticsFactory(context);
        var root = model.SyntaxTree
            .GetCompilationUnitRoot(token);
        var diagnostics = root.DescendantNodes()
            .OfType<LocalDeclarationStatementSyntax>()
            .SelectMany(toImplicitTypeDiagnostics)
            .ToList();
        foreach (var d in diagnostics)
        {
            context.ReportDiagnostic(d);
        }
    }

    private static Func<LocalDeclarationStatementSyntax,
            IEnumerable<Diagnostic>>
        NewDiagnosticsFactory(SemanticModelAnalysisContext context)
    {
        static bool IsTypeDisposable(ITypeSymbol s)
            => s.SpecialType is SpecialType.System_IDisposable;

        static bool DoesImplementDisposable(ITypeSymbol s)
        {
            var allInterfaces = s.AllInterfaces;
            return allInterfaces.Length > 0
                && allInterfaces.Any(IsTypeDisposable);
        }

        static bool IsTrulyDisposableSymbol(ITypeSymbol s)
            => s.TypeKind is TypeKind.Interface
                ? IsTypeDisposable(s)
                    || DoesImplementDisposable(s)
                : DoesImplementDisposable(s)
                    && !Classes.DisposesNothing(
                        TypeSymbols.GetFullNameWithoutNullability(s));

        static bool IsTrulyDisposable(TypeInfo typeInfo)
            => typeInfo.Type is {} type
                && IsTrulyDisposableSymbol(type);

        static IdentifierNameSyntax? GetTypeIdentifier(TypeSyntax s)
        {
            return s is NullableTypeSyntax nullableType
                ? GetTypeIdentifier(nullableType.ElementType)
                : s is IdentifierNameSyntax identifier
                ? identifier
                : null;
        }

        static bool IsNewOperatorUsed(ExpressionSyntax s)
            => s is ObjectCreationExpressionSyntax
                || s is ImplicitObjectCreationExpressionSyntax;

        static ExpressionSyntax? ToNewExpr(VariableDeclaratorSyntax s)
            => s.Initializer?.Value is {} value
                   && IsNewOperatorUsed(value)
                ? value
                : null;

        static bool CanUseUsingPotentially(
                IdentifierNameSyntax s,
                SeparatedSyntaxList<VariableDeclaratorSyntax> variables,
                Func<SyntaxNode, TypeInfo> toTypeInfo)
            => !s.IsVar
                /* Explicit declaration */
                ? !variables.All(i => ToNewExpr(i) is not null)
                    || !IsTrulyDisposable(toTypeInfo(s))
                /* Implicit declaaation */
                : variables.Count < 1
                    || ToNewExpr(variables[0]) is not {} value
                    || !IsTrulyDisposable(toTypeInfo(value));

        return s =>
        {
            var toTypeInfo = context.GetTypeInfoSupplier();
            var declaration = s.Declaration;
            var variables = declaration.Variables;
            var id = declaration.Type;
            if (s.UsingKeyword != default
                || GetTypeIdentifier(id) is not {} typeId
                || CanUseUsingPotentially(typeId, variables, toTypeInfo)
                || IsDeclarationSingleAndToBeReturned(context, s)
                || IsReassigned(context, s)
                || UsedAsParameterOrAssignedToAny(context, s))
            {
                return [];
            }
            var location = id.GetLocation();
            var d = Diagnostic.Create(Rule, location, id);
            return [d];
        };
    }

    private static IEnumerable<Func<BlockSyntax?>>
            ToBlockSuppliers(SyntaxNode i)
        /*
            We can ignore the ExpressionBody property because var (using var)
            cannot exist in an expression.
        */
        => i is BaseMethodDeclarationSyntax method
            ? [() => method.Body]
            : i is LocalFunctionStatementSyntax localFunction
            ? [() => localFunction.Body]
            : i is AnonymousFunctionExpressionSyntax lambda
            ? [() => lambda.Block]
            : [];

    private static BlockSyntax?
            ToContainingBody(LocalDeclarationStatementSyntax s)
        => s.Ancestors()
            .SelectMany(ToBlockSuppliers)
            .FirstOrDefault() is not {} bodyProvider
            ? null
            : bodyProvider();

    private static bool IsDeclarationSingleAndToBeReturned(
        SemanticModelAnalysisContext context,
        LocalDeclarationStatementSyntax s)
    {
        var model = context.SemanticModel;
        var cancellationToken = context.CancellationToken;
        var symbolizer = context.GetSymbolizer();

        bool DoesReturnStatementReturnSymbol(
                ReturnStatementSyntax r, ISymbol symbol)
            => r.Expression is {} expr
                && model.GetSymbolInfo(expr, cancellationToken)
                    .Symbol is {} s
                && SymbolEquals(s, symbol);

        var declaration = s.Declaration;
        var variables = declaration.Variables;
        return variables.Count is 1
            && symbolizer.ToSymbol(variables[0]) is {} symbol
            && ToContainingBody(s) is {} body
            && model.AnalyzeControlFlow(body) is {} controlFlow
            && controlFlow.Succeeded
            && controlFlow.ReturnStatements
                .OfType<ReturnStatementSyntax>()
                .Any(s => DoesReturnStatementReturnSymbol(s, symbol));
    }

    private static bool IsReassigned(
        SemanticModelAnalysisContext context,
        LocalDeclarationStatementSyntax s)
    {
        var model = context.SemanticModel;
        var symbolizer = context.GetSymbolizer();
        var declaration = s.Declaration;
        var variables = declaration.Variables;
        return ToContainingBody(s) is {} body
            && model.AnalyzeDataFlow(s) is {} dataFlow
            && dataFlow.Succeeded
            && variables.Count > 0
            && variables.Any(v => symbolizer.ToSymbol(v) is {} symbol
                && ContainsSymbol(
                    dataFlow.WrittenOutside.Concat(dataFlow.Captured),
                    symbol));
    }

    private static bool ContainsSymbol(IEnumerable<ISymbol> all, ISymbol s)
        => all.Any(i => SymbolEquals(i, s));

    private static bool IsUsedAs<T>(IOperation o)
    {
        var parent = o.Parent;
        var target = parent is IConversionOperation
            /* for implicit conversions */
            ? parent.Parent
            : parent;
        return target is T;
    }

    private static bool IsUsedAsArgument(IOperation o)
        => IsUsedAs<IArgumentOperation>(o);

    private static bool IsUsedAsElement(IOperation o)
        => IsUsedAs<ICollectionExpressionOperation>(o);

    private static bool IsUsedAsTupleElement(IOperation o)
        => IsUsedAs<ITupleOperation>(o);

    private static bool IsUsedAsAssignmentValue(IOperation o)
    {
        var parent = o.Parent;
        return (parent is ISimpleAssignmentOperation a
            && ReferenceEquals(a.Value, o))
            /* for implicit conversions */
            || (parent is IConversionOperation
                && parent.Parent is ISimpleAssignmentOperation b
                && ReferenceEquals(b.Value, parent));
    }

    private static bool UsedAsParameterOrAssignedToAny(
        SemanticModelAnalysisContext context,
        LocalDeclarationStatementSyntax s)
    {
        static bool Predicate(ILocalReferenceOperation o)
            => IsUsedAsArgument(o)
                || IsUsedAsElement(o)
                || IsUsedAsTupleElement(o)
                || IsUsedAsAssignmentValue(o);

        return IsUsedAs(context, s, Predicate);
    }

    private static bool IsUsedAs(
        SemanticModelAnalysisContext context,
        LocalDeclarationStatementSyntax s,
        Func<ILocalReferenceOperation, bool> isUsed)
    {
        var model = context.SemanticModel;
        var cancellationToken = context.CancellationToken;
        var symbolizer = context.GetSymbolizer();
        var declaration = s.Declaration;
        var variables = declaration.Variables;
        return ToContainingBody(s) is {} body
            && variables.Count > 0
            && model.GetOperation(body, cancellationToken) is {} bodyOperation
            && variables.Any(v => symbolizer.ToSymbol(v) is {} symbol
                && bodyOperation.DescendantsAndSelf()
                    .OfType<ILocalReferenceOperation>()
                    .Where(o => SymbolEquals(o.Local, symbol))
                    .Any(isUsed));
    }
}
