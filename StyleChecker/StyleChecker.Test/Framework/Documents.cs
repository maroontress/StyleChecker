namespace StyleChecker.Test.Framework
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Provides the utility methods for <c>Document</c>s.
    /// </summary>
    public sealed class Documents
    {
        /// <summary>
        /// Gets the existing compiler diagnostics on the specified document.
        /// </summary>
        /// <param name="document">
        /// The <c>Document</c> to run the compiler diagnostic analyzers on.
        /// </param>
        /// <returns>
        /// The compiler diagnostics that were found in the code.
        /// </returns>
        public static IEnumerable<Diagnostic> GetCompilerDiagnostics(
            Document document)
        {
            return document.GetSemanticModelAsync().Result.GetDiagnostics();
        }
    }
}
