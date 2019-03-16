namespace StyleChecker.Naming.ThoughtlessName
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
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
    public sealed class Analyzer : DiagnosticAnalyzer
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

        private ConfigPod pod;

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            ConfigBank.LoadRootConfig(context, pod => this.pod = pod);
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
            if (!typeArconym.Equals(name))
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
                if (string.Compare(name, 0, typeName, 0, count) != 0)
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

        private void AnalyzeModel(
            SemanticModelAnalysisContext context)
        {
            var config = pod.RootConfig.ThoughtlessName;
            var disallowedIdentifierSet
                = config.GetDisallowedIdentifiers().ToImmutableHashSet();
            var cancellationToken = context.CancellationToken;
            var model = context.SemanticModel;
            var root = model.SyntaxTree.GetCompilationUnitRoot(
                cancellationToken);

            T ToNodeOf<T>(ISymbol s)
                where T : SyntaxNode
            {
                var reference = s.DeclaringSyntaxReferences.FirstOrDefault();
                return (reference is null) ? null : reference.GetSyntax() as T;
            }

            var locals = LocalVariables.Symbols(model)
                .Select(s => (s.token, s.symbol as ISymbol, s.symbol.Type));
            var parameters = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Select(s => model.GetDeclaredSymbol(s))
                .SelectMany(s => s.Parameters)
                .Select(p => (param: p, node: ToNodeOf<ParameterSyntax>(p)))
                .Where(c => !(c.node is null))
                .Select(c => (c.param, token: c.node.Identifier))
                .Select(c => (c.token, c.param as ISymbol, c.param.Type));
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
    }
}
