namespace StyleChecker.Cleaning.RedundantTypedArrayCreation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Operations;
    using StyleChecker.Refactoring;
    using R = Resources;

    /// <summary>
    /// RedundantTypedArrayCreation analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "RedundantTypedArrayCreation";

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
            bool HasAncestor(ITypeSymbol t, ITypeSymbol u)
            {
                var v = u.BaseType;
                while (v != null)
                {
                    if (v.Equals(t))
                    {
                        return true;
                    }
                    v = v.BaseType;
                }
                return false;
            }

            bool IsAncestorOfAll(ITypeSymbol t, IEnumerable<ITypeSymbol> a)
                => !a.Where(u => !u.Equals(t) && !HasAncestor(t, u)).Any();

            ITypeSymbol ToRawType(IOperation o)
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
                    ? conversion.Operand.Type
                    : o.Type;

            bool HasNull(Optional<object> v)
                => v.HasValue && v.Value == null;

            bool NotNullLiteral(IOperation o)
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
                => !(o is IConversionOperation convertion
                    && o.IsImplicit
                    && convertion.Operand is ILiteralOperation literal
                    && HasNull(literal.ConstantValue));

            IEnumerable<IOperation> ToFlat(IOperation o)
            {
                return !(o is IArrayInitializerOperation a)
                    ? Enumerables.Of(o)
                    : a.ElementValues.SelectMany(ToFlat);
            }

            ITypeSymbol GetTypeSymbolOfElements(
                IArrayCreationOperation newArray)
            {
                var typeSet = newArray.Initializer.ElementValues
                    .Where(NotNullLiteral)
                    .SelectMany(ToFlat)
                    .Select(ToRawType)
                    .ToImmutableHashSet();
                return (typeSet.Any() && typeSet.Count == 1)
                    ? typeSet.First()
                    : typeSet.Where(t => IsAncestorOfAll(t, typeSet))
                        .FirstOrDefault();
            }

            bool CanBeImplicit(IArrayCreationOperation newArray)
            {
                var elementType = GetTypeSymbolOfElements(newArray);
                return elementType != null
                    && newArray.Type is IArrayTypeSymbol arrayType
                    && arrayType.ElementType.Equals(elementType);
            }

            bool NotAllOmmited(ArrayRankSpecifierSyntax n)
                => n.Sizes
                    .Where(e => !(e is OmittedArraySizeExpressionSyntax))
                    .Any();

            bool IsOmmitedArraySize(ArrayCreationExpressionSyntax n)
                => !n.Type.RankSpecifiers.Where(NotAllOmmited).Any();

            var root = context.GetCompilationUnitRoot();
            var model = context.SemanticModel;
            var all = root.DescendantNodes()
                .OfType<ArrayCreationExpressionSyntax>()
                .Where(IsOmmitedArraySize)
                .Select(n => model.GetOperation(n))
                .OfType<IArrayCreationOperation>()
                .Where(CanBeImplicit);

            foreach (var o in all)
            {
                if (!(o.Syntax is ArrayCreationExpressionSyntax node))
                {
                    continue;
                }
                var type = node.Type.ElementType;
                var diagnostic = Diagnostic.Create(
                    Rule,
                    type.GetLocation(),
                    type);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
