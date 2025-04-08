namespace Analyzers.Naming.ThoughtlessName;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzers.Settings;
using Maroontress.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using R = Resources;

/// <summary>
/// ThoughtlessName analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "ThoughtlessName";

    private const string Category = Categories.Naming;

    private static readonly DiagnosticDescriptor Rule = NewRule();

    private static readonly ImmutableHashSet<SpecialType>
            SinglePrefixTypeSet = [
        SpecialType.System_Boolean,
        SpecialType.System_Byte,
        SpecialType.System_Int16,
        SpecialType.System_Char,
        SpecialType.System_Int32,
        SpecialType.System_Int64,
        SpecialType.System_Single,
        SpecialType.System_Double,
        SpecialType.System_Object,
        SpecialType.System_String,
        SpecialType.System_Decimal];

    private static readonly ImmutableHashSet<SpecialType>
            DoublePrefixTypeSet = [
        SpecialType.System_SByte,
        SpecialType.System_UInt16,
        SpecialType.System_UInt32,
        SpecialType.System_UInt64];

    private static readonly IReadOnlyDictionary<SpecialType, string>
        SpecialTypeNameMap = new Dictionary<SpecialType, string>()
    {
        [SpecialType.System_Object] = "object",
        [SpecialType.System_Boolean] = "bool",
        [SpecialType.System_Char] = "char",
        [SpecialType.System_SByte] = "sbyte",
        [SpecialType.System_Byte] = "byte",
        [SpecialType.System_Int16] = "short",
        [SpecialType.System_UInt16] = "ushort",
        [SpecialType.System_Int32] = "int",
        [SpecialType.System_UInt32] = "uint",
        [SpecialType.System_Int64] = "long",
        [SpecialType.System_UInt64] = "ulong",
        [SpecialType.System_Decimal] = "decimal",
        [SpecialType.System_Single] = "float",
        [SpecialType.System_Double] = "double",
        [SpecialType.System_String] = "string",
    };

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        ConfigBank.LoadRootConfig(context, StartAction);
        context.EnableConcurrentExecution();
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

    private void Check(
        ISymbol symbol, ITypeSymbol typeSymbol, Action<string> action)
    {
        IfArconym(symbol, typeSymbol, action);
        IfHungarianPrefix(symbol, typeSymbol, action);
    }

    private void IfArconym(
        ISymbol symbol, ITypeSymbol typeSymbol, Action<string> action)
    {
        var name = symbol.Name;
        if (name.Length is 1)
        {
            return;
        }
        var typeName = typeSymbol.Name;
        var typeArconym = new string(
            [.. typeName.ToCharArray()
                .Where(c => char.IsUpper(c))
                .Select(c => char.ToLower(c))]);
        if (typeArconym != name)
        {
            return;
        }
        var reason = string.Format(CompilerCulture, R.Acronym, name, typeName);
        action(reason);
    }

    private void IfHungarianPrefix(
        ISymbol symbol, ITypeSymbol typeSymbol, Action<string> action)
    {
        static bool StartsWithTheSame(string s1, string s2, int length)
            => string.Compare(s1, 0, s2, 0, length, StringComparison.Ordinal)
                is 0;

        void PerformIf(int count, Func<SpecialType, bool> nameTypeF)
        {
            var name = symbol.Name;
            if (name.Length <= count
                || name.Take(count).Any(char.IsUpper)
                || char.IsLower(name[count]))
            {
                return;
            }
            var specialType = typeSymbol.SpecialType;
            if (nameTypeF(specialType))
            {
                return;
            }
            var typeName = SpecialTypeNameMap[specialType];
            if (!StartsWithTheSame(name, typeName, count))
            {
                return;
            }
            var reason = string.Format(
                CompilerCulture, R.HungarianPrefix, name, typeName);
            action(reason);
        }
        PerformIf(1, t => !SinglePrefixTypeSet.Contains(t));
        PerformIf(2, t => !DoublePrefixTypeSet.Contains(t));
    }

    private void AnalyzeModel(
        SemanticModelAnalysisContext context, ConfigPod pod)
    {
        var config = pod.RootConfig
            .ThoughtlessName;
        var disallowedIdentifierSet = config.GetDisallowedIdentifiers()
            .ToImmutableHashSet();
        var cancellationToken = context.CancellationToken;
        var model = context.SemanticModel;
        var root = model.SyntaxTree.GetCompilationUnitRoot(cancellationToken);

        static T? ToNodeOf<T>(ISymbol s)
            where T : SyntaxNode
        {
            return s.DeclaringSyntaxReferences
                .FirstOrDefault()
                ?.GetSyntax() as T;
        }

        static IEnumerable<(T Symbol, U Node)> ToPairs<T, U>(T p)
            where T : ISymbol
            where U : SyntaxNode
        {
            return ToNodeOf<U>(p) is {} node
                ? [(p, node)]
                : [];
        }

        var locals = LocalVariables.Symbols(model)
            .Select(s => (s.Token, s.Symbol as ISymbol, s.Symbol.Type));
        var parameters = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Select(s => model.GetDeclaredSymbol(s))
            .FilterNonNullReference()
            .SelectMany(s => s.Parameters)
            .SelectMany(ToPairs<IParameterSymbol, ParameterSyntax>)
            .Select(c
                => (c.Node.Identifier, c.Symbol as ISymbol, c.Symbol.Type));
        var all = locals.Concat(parameters)
            .ToList();
        foreach (var (token, symbol, typeSymbol) in all)
        {
            void ReportDiagnostic(string reason)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    token.GetLocation(),
                    token,
                    reason);
                context.ReportDiagnostic(diagnostic);
            }

            Check(symbol, typeSymbol, ReportDiagnostic);

            if (disallowedIdentifierSet.Contains(symbol.Name))
            {
                ReportDiagnostic(R.Disallowed);
            }
        }
    }

    private void StartAction(
        CompilationStartAnalysisContext context, ConfigPod pod)
    {
        context.RegisterSemanticModelAction(c => AnalyzeModel(c, pod));
    }
}
