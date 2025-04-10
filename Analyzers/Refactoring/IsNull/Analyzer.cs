namespace Analyzers.Refactoring.IsNull;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using R = Resources;

/// <summary>
/// IsNull analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "IsNull";

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
        static bool IsNullConstant(Optional<object?> v)
            => v.HasValue && v.Value is null;

        static bool IsNullLiteral(IOperation o)
            => (o.IsImplicit && o is IConversionOperation conversion)
                ? IsNullLiteral(conversion.Operand)
                : (o is ILiteralOperation literal
                   && IsNullConstant(literal.ConstantValue));

        static bool IsNull(IPatternOperation o)
            => o is IConstantPatternOperation constantPattern
                && IsNullLiteral(constantPattern.Value);

        static bool Matches(IIsPatternOperation o)
            => IsNull(o.Pattern);

        static Diagnostic Of(SyntaxNode n, SyntaxKind k)
            => Diagnostic.Create(
                Rule, n.GetLocation(), SyntaxFactory.Token(k));

        static Diagnostic New(IsPatternExpressionSyntax node)
            => (node.Parent is ParenthesizedExpressionSyntax paren
                && paren.Parent is PrefixUnaryExpressionSyntax prefix
                && prefix.OperatorToken.IsKind(SyntaxKind.ExclamationToken))
                ? Of(prefix, SyntaxKind.ExclamationEqualsToken)
                : Of(node, SyntaxKind.EqualsEqualsToken);

        var root = context.GetCompilationUnitRoot();
        var model = context.SemanticModel;
        var all = root.DescendantNodes()
            .OfType<IsPatternExpressionSyntax>()
            .Select(n => model.GetOperation(n))
            .OfType<IIsPatternOperation>()
            .Where(Matches)
            .Select(o => o.Syntax)
            .OfType<IsPatternExpressionSyntax>()
            .ToList();
        foreach (var node in all)
        {
            context.ReportDiagnostic(New(node));
        }
    }
}
