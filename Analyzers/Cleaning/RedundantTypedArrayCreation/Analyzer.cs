namespace Analyzers.Cleaning.RedundantTypedArrayCreation;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeDebt.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using R = Resources;

/// <summary>
/// RedundantTypedArrayCreation analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "RedundantTypedArrayCreation";

    private const string Category = Categories.Cleaning;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

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
        static bool HasAncestor(ITypeSymbol t, ITypeSymbol u)
        {
            var v = u.BaseType;
            while (v is not null)
            {
                if (Symbols.AreEqual(v, t))
                {
                    return true;
                }
                v = v.BaseType;
            }
            return false;
        }

        static bool IsAncestorOfAll(
                ITypeSymbol t, IEnumerable<ITypeSymbol> a)
            => !a.Any(u => !SymbolEqualityComparer.Default.Equals(u, t)
                && !HasAncestor(t, u));

        static ITypeSymbol? ToRawType(IOperation o)
            /*
                1. new object[] = { "a", ... };
                                    ^^^
                   IConversionOperation (implicit, object)
                   + ILiteralOperation ("a")

                2. new string[] = { "a", ... };
                                    ^^^
                   ILiteralOperation ("a")

                This function returns the symbol of 'string' in the case
                1 and 2.
            */
            => (o.IsImplicit && o is IConversionOperation conversion)
                /*
                    `conversion.Operand.Type` can be null when the type
                    inference of the operand does not work (e.g., method
                    references).
                */
                ? conversion.Operand.Type
                : o.Type;

        static bool HasNull(Optional<object?> v)
            => v.HasValue && v.Value is null;

        static bool IsNullLiteral(IOperation o)
            /*
                1. new string[] = { null, ... };
                                    ^^^^
                    IConversionOperation (implicit, string)
                    + ILiteralOperation (null)

                2. new string[] = { (string)null, ... };
                                            ^^^^
                    IConversionOperation (explicit, string)
                    + ILiteralOperation (null)

                3. new string[] = { "a", ... };
                                    ^^^
                    ILiteralOperation ("a")

                This function returns false in the case 1, true otherwise.
            */
            => o is IConversionOperation convertion
                && o.IsImplicit
                && convertion.Operand is ILiteralOperation literal
                && HasNull(literal.ConstantValue);

        static bool IsDefaultLiteral(IOperation o)
            => o.Syntax is LiteralExpressionSyntax s
                && s.IsKind(SyntaxKind.DefaultLiteralExpression);

        static bool IsImplicitNew(IOperation o)
            => o.Syntax is ImplicitObjectCreationExpressionSyntax;

        static bool IsCollectionExpression(IOperation o)
            => o.Syntax is CollectionExpressionSyntax;

        static IEnumerable<IOperation> ToFlat(IOperation o)
        {
            return o is not IArrayInitializerOperation a
                ? [o]
                : a.ElementValues.SelectMany(ToFlat);
        }

        static bool IsSystemNullableType(ITypeSymbol s)
            => s.OriginalDefinition.SpecialType
                is SpecialType.System_Nullable_T;

        static ITypeSymbol? GetTypeSymbolOfElements(
            IArrayCreationOperation newArray)
        {
            if (newArray.Initializer is not {} initializer)
            {
                /*
                    The following code results in a compilation error.

                    var array = new string[];

                    And it makes the Initializer property return null.
                */
                return null;
            }
            var rawTypeSet = initializer.ElementValues
                .SelectMany(ToFlat)
                .Select(ToRawType)
                .FilterNonNullReference()
                .ToFrozenSet();
            var excludeTypeSet = rawTypeSet.Where(IsSystemNullableType)
                .SelectMany(s => s is INamedTypeSymbol n
                    ? n.TypeArguments
                    : [])
                .ToRigidSet();
            var typeSet = rawTypeSet.Where(s => !excludeTypeSet.Contains(s))
                .ToFrozenSet();
            return (typeSet.Count is 1)
                ? typeSet.First()
                : typeSet.FirstOrDefault(t => IsAncestorOfAll(t, typeSet));
        }

        static bool WrapsMethodReference(IDelegateCreationOperation o)
        {
            var firstChild = o.ChildOperations.FirstOrDefault();
            return firstChild is IMethodReferenceOperation;
        }

        static bool ForReferenceType(IOperation o)
            => IsNullLiteral(o)
                || IsDefaultLiteral(o)
                || IsImplicitNew(o)
                || IsCollectionExpression(o);

        static bool ForNullableValueType(IOperation o)
            => ForReferenceType(o);

        static bool ForNonNullableValueType(IOperation o)
            => IsDefaultLiteral(o)
                || IsImplicitNew(o)
                || IsCollectionExpression(o);

        static Func<IOperation, bool> ValueTypePredicate(ITypeSymbol type)
            => type.NullableAnnotation is NullableAnnotation.Annotated
                ? ForNullableValueType
                : ForNonNullableValueType;

        static bool ForMethodReference(IArrayInitializerOperation o)
        {
            return o.ElementValues
                .Where(i => !IsNullLiteral(i)
                    && !IsDefaultLiteral(i)
                    && !IsImplicitNew(i))
                /* Enumerable.Empty<T>().All(...) is always true */
                .All(o => o is IDelegateCreationOperation c
                    && c.IsImplicit
                    && WrapsMethodReference(c));
        }

        static bool CheckTypeParameter(
            ITypeParameterSymbol s,
            IEnumerable<IOperation>　values)
        {
            return s.HasReferenceTypeConstraint
                ? values.All(ForReferenceType)
                : s.HasValueTypeConstraint
                ? values.All(ValueTypePredicate(s))
                : values.All(ForNonNullableValueType);
        }

        static bool AreAllElementsUnableToInferType(
            IArrayCreationOperation newArray,
            LanguageVersion langVersion)
        {
            if (newArray.Initializer is not {} initializer
                || newArray.Type is not IArrayTypeSymbol arrayType)
            {
                return true;
            }
            var elementType = arrayType.ElementType;
            var values = initializer.ElementValues;
            var result = (elementType is ITypeParameterSymbol typeParameter)
                ? CheckTypeParameter(typeParameter, values)
                : elementType switch
                {
                    { IsReferenceType: true }
                        => values.All(ForReferenceType),
                    { IsValueType: true }
                        => values.All(ValueTypePredicate(elementType)),
                    _ => true,
                };
            return result
                || (langVersion < LanguageVersion.CSharp10
                    && ForMethodReference(initializer));
        }

        static bool CanBeImplicit(IArrayCreationOperation newArray)
        {
            if (newArray.SemanticModel is not {} model
                || model.Compilation.SyntaxTrees.FirstOrDefault()?.Options
                    is not CSharpParseOptions cSharpParsesOptions)
            {
                return false;
            }
            var langVersion = cSharpParsesOptions.LanguageVersion;
            return !AreAllElementsUnableToInferType(newArray, langVersion)
                && GetTypeSymbolOfElements(newArray) is {} elementType
                && newArray.Type is IArrayTypeSymbol arrayType
                && Symbols.AreEqual(arrayType.ElementType, elementType);
        }

        static bool NotAllOmmited(ArrayRankSpecifierSyntax n)
            => n.Sizes
                .Any(e => e is not OmittedArraySizeExpressionSyntax);

        static bool IsOmmitedArraySize(ArrayCreationExpressionSyntax n)
            => !n.Type.RankSpecifiers.Any(NotAllOmmited);

        var root = context.GetCompilationUnitRoot();
        var model = context.SemanticModel;
        var all = root.DescendantNodes()
            .OfType<ArrayCreationExpressionSyntax>()
            .Where(IsOmmitedArraySize)
            .Select(n => model.GetOperation(n))
            .OfType<IArrayCreationOperation>()
            .Where(CanBeImplicit)
            .Select(o => o.Syntax)
            .OfType<ArrayCreationExpressionSyntax>()
            .ToList();

        foreach (var node in all)
        {
            var type = node.Type;
            var diagnostic = Diagnostic.Create(
                Rule,
                type.GetLocation(),
                type,
                type.RankSpecifiers.Last());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
