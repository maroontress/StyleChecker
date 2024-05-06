namespace StyleChecker.Naming.SingleTypeParameter;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using R = Resources;

/// <summary>
/// SingleTypeParameter analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "SingleTypeParameter";

    private const string Category = Categories.Naming;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
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

    private static IEnumerable<SyntaxToken> ToToken(
        TypeParameterListSyntax node)
    {
        var p = node.Parameters;
        if (p.Count is not 1
            || node.Parent is not {} container
            || container.DescendantTokens()
                .Any(t => t.ValueText is "T"
                    && t.Parent is IdentifierNameSyntax)
            || (node.Parent is TypeDeclarationSyntax parent
                && parent.Identifier.ValueText is "T"))
        {
            return [];
        }
        var token = p[0].Identifier;
        return token.ValueText is "T" ? [] : [token];
    }

    private static void AnalyzeSyntaxTree(
        SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree
            .GetCompilationUnitRoot(context.CancellationToken);
        var all = root.DescendantNodes()
            .OfType<TypeParameterListSyntax>()
            .SelectMany(ToToken);
        foreach (var token in all)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                token.GetLocation(),
                token);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
