namespace StyleChecker.Refactoring.IneffectiveReadByte
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;
    using R = Resources;

    /// <summary>
    /// PostIncrement code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixer))]
    [Shared]
    public sealed class CodeFixer : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(Analyzer.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(
            CodeFixContext context)
        {
            var localize = Localizers.Of(R.ResourceManager, typeof(R));
            var title = localize(nameof(R.FixTitle)).ToString();

            var root = await context
                .Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var properties = diagnostic.Properties;

            var node = root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument:
                        c => Replace(context.Document, node, properties, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> Replace(
            Document document,
            SyntaxNode node,
            ImmutableDictionary<string, string> properties,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            var formatAnnotation = Formatter.Annotation;

            var statement = Substitute(R.FixTemplate, properties);
            var newNode = SyntaxFactory.ParseStatement(statement)
                .WithTriviaFrom(node)
                .WithAdditionalAnnotations(formatAnnotation);

            var solution = document.Project.Solution;
            var workspace = solution.Workspace;

            var formattedNode = Formatter.Format(
               newNode,
               formatAnnotation,
               workspace,
               workspace.Options);
            var newRoot = root.ReplaceNode(node, formattedNode);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        private string Substitute(
            string template, ImmutableDictionary<string, string> map)
        {
            var @in = new StringReader(template);
            int Read() => @in.Read();
            char ReadChar()
            {
                var c = @in.Read();
                if (c == -1)
                {
                    throw new EndOfStreamException();
                }
                return (char)c;
            }

            var b = new StringBuilder(template.Length);
            for (;;)
            {
                var o = Read();
                if (o == -1)
                {
                    break;
                }
                var c = (char)o;
                if (c != '$')
                {
                    b.Append(c);
                    continue;
                }
                c = ReadChar();
                if (c != '{')
                {
                    b.Append('$');
                    b.Append(c);
                    continue;
                }
                var keyBuilder = new StringBuilder();
                for (;;)
                {
                    c = ReadChar();
                    if (c == '}')
                    {
                        break;
                    }
                    keyBuilder.Append(c);
                }
                var key = keyBuilder.ToString();
                b.Append(map[key]);
            }
            return b.ToString();
        }
    }
}
