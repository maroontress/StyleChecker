namespace TestHelper
{
    using Microsoft.CodeAnalysis;

    public struct VerifierContext
    {
        public Document[] Documents;
        public Diagnostic[] CompilerDiagnostics;
        public Diagnostic[] AnalyzerDiagnostics;
    }
}
