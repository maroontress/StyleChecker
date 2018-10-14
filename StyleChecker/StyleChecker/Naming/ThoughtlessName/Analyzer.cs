namespace StyleChecker.Naming.ThoughtlessName
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
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

        private static readonly DiagnosticDescriptor Rule;

        private static readonly
            ImmutableHashSet<SpecialType> SinglePrefixTypeSet;

        private static readonly
            ImmutableHashSet<SpecialType> DoublePrefixTypeSet;

        private static readonly
            ImmutableDictionary<SpecialType, string> SpecialTypeNameMap;

        static Analyzer()
        {
            var localize = Localizers.Of(R.ResourceManager, typeof(R));
            Rule = new DiagnosticDescriptor(
                DiagnosticId,
                localize(nameof(R.Title)),
                localize(nameof(R.MessageFormat)),
                Category,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: localize(nameof(R.Description)));
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
            DoublePrefixTypeSet = new HashSet<SpecialType>()
            {
                SpecialType.System_SByte,
                SpecialType.System_UInt16,
                SpecialType.System_UInt32,
                SpecialType.System_UInt64,
            }.ToImmutableHashSet();
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
        }

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

        private static void AnalyzeModel(
            SemanticModelAnalysisContext context)
        {
            var model = context.SemanticModel;
            var root = model.SyntaxTree.GetCompilationUnitRoot(
                context.CancellationToken);
            var all = LocalVariables.DeclarationTokens(root)
                .Concat(LocalVariables.DesignationTokens(root))
                .Concat(LocalVariables.DesignationTokens(root))
                .Concat(LocalVariables.ParameterTokens(root))
                .ToList();

            if (all.Count == 0)
            {
                return;
            }
            foreach (var token in all)
            {
                var name = token.Text;
                var span = token.Span;
                var symbol = model.LookupSymbols(span.Start, null, name)
                    .FirstOrDefault() as ILocalSymbol;
                if (symbol == null)
                {
                    continue;
                }

                void ReportDiagnostic(string reason)
                {
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        token.GetLocation(),
                        token,
                        reason);
                    context.ReportDiagnostic(diagnostic);
                }
                IfArconym(symbol, ReportDiagnostic);
                IfHungarianPrefix(symbol, ReportDiagnostic);
            }
        }

        private static void IfArconym(
            ILocalSymbol symbol, Action<string> action)
        {
            var name = symbol.Name;
            if (name.Length == 1)
            {
                return;
            }
            var typeSymbol = symbol.Type;
            var typeName = typeSymbol.Name;
            var typeArconym = new string(typeName.ToCharArray()
                .Where(c => char.IsUpper(c))
                .Select(c => char.ToLower(c))
                .ToArray());
            if (!typeArconym.Equals(name))
            {
                return;
            }
            var reason = string.Format(R.Acronym, name, typeName);
            action(reason);
        }

        private static void IfHungarianPrefix(
            ILocalSymbol symbol, Action<string> action)
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
                var typeSymbol = symbol.Type;
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
                var reason = string.Format(R.HungarianPrefix, name, typeName);
                action(reason);
            }
            PerformIf(1, t => !SinglePrefixTypeSet.Contains(t));
            PerformIf(2, t => !DoublePrefixTypeSet.Contains(t));
        }
    }
}
