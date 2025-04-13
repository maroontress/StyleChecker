namespace StyleChecker.Analyzers.Naming.SingleTypeParameter;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using StyleChecker.Analyzers;
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

    private static void AnalyzeModel(SemanticModelAnalysisContext context)
    {
        static Func<TypeParameterListSyntax, IEnumerable<TypeParameterSyntax>>
                RenameableNodesSupplier(ModelKit kit)
            => n => ToRenameableNodes(kit, n);

        var kit = new ModelKit(context);
        var root = kit.GetCompilationUnitRoot();
        var toRenameableNodes = RenameableNodesSupplier(kit);
        var allTokens = root.DescendantNodes()
            .OfType<TypeParameterListSyntax>()
            .SelectMany(toRenameableNodes)
            .Select(n => n.Identifier)
            .ToList();
        foreach (var t in allTokens)
        {
            var d = Diagnostic.Create(Rule, t.GetLocation(), t);
            context.ReportDiagnostic(d);
        }
    }

    private static IEnumerable<TypeParameterSyntax> ToRenameableNodes(
        ModelKit kit, TypeParameterListSyntax typeParameterList)
    {
        return typeParameterList.Parameters is not { Count: 1 } parameters
                || parameters[0] is not { Identifier.Text: not "T" }
                || typeParameterList.Parent is not {} container
                || HasAncestorWithTypeParameterT(container)
            ? []
            : container switch
            {
                TypeDeclarationSyntax type
                    => CanTypeAcceptRenaming(kit, type)
                        ? parameters
                        : [],
                MemberDeclarationSyntax member
                        when CanMemberAcceptRenaming(member)
                    => parameters,
                _ => [],
            };
    }

    private static IEnumerable<SyntaxToken> DescendantIdentifierTokens(
            SyntaxNode node)
        => node.DescendantTokens()
            .Where(t => t.IsKind(SyntaxKind.IdentifierToken));

    private static bool CanMemberAcceptRenaming(MemberDeclarationSyntax member)
        => DescendantIdentifierTokens(member)
            .All(t => t.Text is not "T");

    private static IEnumerable<SyntaxToken> GetSelfAndMemberIdentifiers(
            TypeDeclarationSyntax type)
        => type.Members
            .SelectMany(ToIdentifiers)
            .Prepend(type.Identifier);

    private static bool CanTypeAcceptRenaming(
        ModelKit kit, TypeDeclarationSyntax type)
    {
        if (GetSelfAndMemberIdentifiers(type).Any(t => t.Text is "T")
            || kit.GetDeclaredSymbol(type) is not {} typeSymbol)
        {
            return false;
        }
        var prefix = typeSymbol.ToString() + ".";
        var descendantTokens = DescendantIdentifierTokens(type).ToList();
        return descendantTokens.Where(t => t.Text is "T")
                .SelectMany(kit.GetFullName)
                .All(n => n.StartsWith(prefix))
            && descendantTokens.Where(t => t.Parent is TypeParameterSyntax)
                .All(t => t.Text is not "T");
    }

    private static bool HasAncestorWithTypeParameterT(SyntaxNode origin)
    {
        var maybeNode = origin.Parent;
        while (maybeNode is {} node)
        {
            if (node is TypeDeclarationSyntax typeDeclaration
                && typeDeclaration.TypeParameterList is {} list
                && list.Parameters.Any(p => p.Identifier.Text is "T"))
            {
                return true;
            }
            maybeNode = node.Parent;
        }
        return false;
    }

    private static IEnumerable<SyntaxToken> ToIdentifiers(
            MemberDeclarationSyntax m)
        => m switch
        {
            MethodDeclarationSyntax method
                => [method.Identifier],
            PropertyDeclarationSyntax property
                => [property.Identifier],
            EventDeclarationSyntax @event
                => [@event.Identifier],
            DelegateDeclarationSyntax @delegate
                => [@delegate.Identifier],
            BaseTypeDeclarationSyntax type
                => [type.Identifier],
            BaseFieldDeclarationSyntax field
                => field.Declaration
                    .Variables
                    .Select(v => v.Identifier),
            _ => [],
        };
}
