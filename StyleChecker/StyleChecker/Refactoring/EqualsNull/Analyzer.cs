namespace StyleChecker.Refactoring.EqualsNull;

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
/// EqualsNull analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "EqualsNull";

    private const string Category = Categories.Refactoring;
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
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: localize(nameof(R.Description)),
            helpLinkUri: HelpLink.ToUri(DiagnosticId));
    }

    private static void AnalyzeModel(
        SemanticModelAnalysisContext context)
    {
        static bool IsEqualOrNotEqual(BinaryOperatorKind k)
            => k is BinaryOperatorKind.Equals
                || k is BinaryOperatorKind.NotEquals;

        static bool IsNull(IOperation o)
        {
            var v = o.ConstantValue;
            return v.HasValue
                && v.Value is null;
        }

        static bool IsNonNullableValueType(ITypeSymbol s)
            => s.IsValueType
                && s.OriginalDefinition is {} d
                && d.SpecialType is not SpecialType.System_Nullable_T;

        static bool CanBeComparedWithNull(IOperation o)
            => (o.IsImplicit && o is IConversionOperation conversion)
                ? CanBeComparedWithNull(conversion.Operand)
                : o.Type is {} type && !IsNonNullableValueType(type);

        static bool Matches(IBinaryOperation o)
            => IsEqualOrNotEqual(o.OperatorKind)
                && CanBeComparedWithNull(o.LeftOperand)
                && IsNull(o.RightOperand);

        var root = context.GetCompilationUnitRoot();
        var model = context.SemanticModel;
        var all = root.DescendantNodes()
            .OfType<BinaryExpressionSyntax>()
            .Select(n => model.GetOperation(n))
            .OfType<IBinaryOperation>()
            .Where(Matches)
            .Select(o => o.Syntax)
            .OfType<BinaryExpressionSyntax>()
            .ToList();

        foreach (var node in all)
        {
            var token = node.OperatorToken;
            var diagnostic = Diagnostic.Create(
                Rule, node.GetLocation(), token);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
