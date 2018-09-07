namespace TestHelper
{
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    public sealed class ConfigText : AdditionalText
    {
        private readonly string text;

        public ConfigText(string text)
        {
            this.text = text;
        }

        public override string Path => "StyleChecker.xml";

        public static AnalyzerOptions ToAnalyzerOptions(string text)
        {
            var additionalFiles = ImmutableArray.Create(
                new ConfigText(text) as AdditionalText);
            return new AnalyzerOptions(additionalFiles);
        }

        public override SourceText GetText(
            CancellationToken cancellationToken)
        {
            return SourceText.From(text);
        }
    }
}
