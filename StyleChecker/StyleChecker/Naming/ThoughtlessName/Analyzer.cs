namespace StyleChecker.Naming.ThoughtlessName
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using Maroontress.Extensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleChecker.Settings;
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
            SinglePrefixTypeSet = new HashSet<SpecialType>()
            {
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
                SpecialType.System_Decimal,
            }.ToImmutableHashSet();

        private static readonly ImmutableHashSet<SpecialType>
            DoublePrefixTypeSet = new HashSet<SpecialType>()
            {
                SpecialType.System_SByte,
                SpecialType.System_UInt16,
                SpecialType.System_UInt32,
                SpecialType.System_UInt64,
            }.ToImmutableHashSet();

        private static readonly ImmutableDictionary<SpecialType, string>
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
            }.ToImmutableDictionary();

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        private static void Check(
            ISymbol symbol, ITypeSymbol typeSymbol, Action<string> action)
        {
            IfArconym(symbol, typeSymbol, action);
            IfHungarianPrefix(symbol, typeSymbol, action);
        }

        private static void IfArconym(
            ISymbol symbol, ITypeSymbol typeSymbol, Action<string> action)
        {
            var name = symbol.Name;
            if (name.Length == 1)
            {
                return;
            }
            var typeName = typeSymbol.Name;
            var typeArconym = new string(typeName.ToCharArray()
                .Where(c => char.IsUpper(c))
                .Select(c => char.ToLower(c))
                .ToArray());
            if (typeArconym != name)
            {
                return;
            }
            var reason = string.Format(
                CultureInfo.CurrentCulture, R.Acronym, name, typeName);
            action(reason);
        }

        private static void IfHungarianPrefix(
            ISymbol symbol, ITypeSymbol typeSymbol, Action<string> action)
        {
            static bool StartsWithTheSame(string s1, string s2, int length)
                => string.Compare(
                    s1, 0, s2, 0, length, StringComparison.Ordinal) == 0;

            void PerformIf(int count, Func<SpecialType, bool> nameTypeF)
            {
                var name = symbol.Name;
                if (name.Length <= count)
                {
                    return;
                }
                for (var k = 0; k < count; ++k)
                {
                    if (char.IsUpper(name[k]))
                    {
                        return;
                    }
                }
                if (char.IsLower(name[count]))
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
                    CultureInfo.CurrentCulture,
                    R.HungarianPrefix,
                    name,
                    typeName);
                action(reason);
            }
            PerformIf(1, t => !SinglePrefixTypeSet.Contains(t));
            PerformIf(2, t => !DoublePrefixTypeSet.Contains(t));
        }

        private static void AnalyzeModel(
            SemanticModelAnalysisContext context,
            ConfigPod pod)
        {
            var config = pod.RootConfig.ThoughtlessName;
            var disallowedIdentifierSet
                = config.GetDisallowedIdentifiers().ToImmutableHashSet();
            var cancellationToken = context.CancellationToken;
            var model = context.SemanticModel;
            var root = model.SyntaxTree.GetCompilationUnitRoot(
                cancellationToken);

            static T? ToNodeOf<T>(ISymbol s)
                where T : SyntaxNode
            {
                var reference = s.DeclaringSyntaxReferences.FirstOrDefault();
                return (reference is null) ? null : reference.GetSyntax() as T;
            }

            static IEnumerable<(T Symbol, U Node)> ToPairs<T, U>(T p)
                where T : ISymbol
                where U : SyntaxNode
            {
                var node = ToNodeOf<U>(p);
                return node is null
                    ? Enumerable.Empty<(T, U)>()
                    : new[] { (p, node) };
            }

            var locals = LocalVariables.Symbols(model)
                .Select(s => (s.Token, s.Symbol as ISymbol, s.Symbol.Type));
            var parameters = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Select(s => model.GetDeclaredSymbol(s))
                .FilterNonNullReference()
                .SelectMany(s => s.Parameters)
                .SelectMany(ToPairs<IParameterSymbol, ParameterSyntax>)
                .Select(c => (c.Symbol, Token: c.Node.Identifier))
                .Select(c => (c.Token, c.Symbol as ISymbol, c.Symbol.Type));
            var all = locals
                .Concat(parameters)
                .ToList();
            if (all.Count == 0)
            {
                return;
            }
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
            context.RegisterSemanticModelAction(
                c => AnalyzeModel(c, pod));
        }
    }
}
