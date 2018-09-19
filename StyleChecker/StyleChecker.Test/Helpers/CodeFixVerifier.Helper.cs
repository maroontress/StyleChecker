namespace TestHelper
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Simplification;

    /// <summary>
    /// Diagnostic Producer class with extra methods dealing with applying
    /// CodeFixes. All methods are static.
    /// </summary>
    public abstract partial class CodeFixVerifier : DiagnosticVerifier
    {
        /// <summary>
        /// Applies the specified <c>CodeAction</c> to the specified document.
        /// Meant to be used to apply code fixes.
        /// </summary>
        /// <param name="document">
        /// The Document to apply the fix on.
        /// </param>
        /// <param name="codeAction">
        /// A CodeAction that will be applied to the Document.
        /// </param>
        /// <returns>
        /// A Document with the changes from the CodeAction.
        /// </returns>
        private static Document ApplyFix(
            Document document, CodeAction codeAction)
        {
            var operations = codeAction
                .GetOperationsAsync(CancellationToken.None)
                .Result;
            var solution = operations
                .OfType<ApplyChangesOperation>()
                .Single()
                .ChangedSolution;
            return solution.GetDocument(document.Id);
        }

        /// <summary>
        /// Compares two collections of <c>Diagnostic</c>s, and returns a list
        /// of any new diagnostics that appear only in the second collection.
        /// Note: Considers <c>Diagnostic</c>s to be the same if they have the
        /// same IDs. In the case of multiple diagnostics with the same ID in a
        /// row, this method may not necessarily return the new one.
        /// </summary>
        /// <param name="diagnostics">
        /// The <c>Diagnostic</c>s that existed in the code before the CodeFix
        /// was applied.
        /// </param>
        /// <param name="newDiagnostics">
        /// The <c>Diagnostic</c>s that exist in the code after the CodeFix was
        /// applied.
        /// </param>
        /// <returns>
        /// A list of <c>Diagnostic</c>s that only surfaced in the code after
        /// the CodeFix was applied.
        /// </returns>
        private static IEnumerable<Diagnostic> GetNewDiagnostics(
            IEnumerable<Diagnostic> diagnostics,
            IEnumerable<Diagnostic> newDiagnostics)
        {
            var oldArray = diagnostics
                .OrderBy(d => d.Location.SourceSpan.Start)
                .ToArray();
            var newArray = newDiagnostics
                .OrderBy(d => d.Location.SourceSpan.Start)
                .ToArray();

            var oldIndex = 0;
            var newIndex = 0;

            while (newIndex < newArray.Length)
            {
                if (oldIndex < oldArray.Length
                    && oldArray[oldIndex].Id == newArray[newIndex].Id)
                {
                    ++oldIndex;
                    ++newIndex;
                }
                else
                {
                    yield return newArray[newIndex++];
                }
            }
        }

        /// <summary>
        /// Gets the existing compiler diagnostics on the specified document.
        /// </summary>
        /// <param name="document">
        /// The <c>Document</c> to run the compiler diagnostic analyzers on.
        /// </param>
        /// <returns>
        /// The compiler diagnostics that were found in the code.
        /// </returns>
        private static IEnumerable<Diagnostic> GetCompilerDiagnostics(
            Document document)
        {
            return document.GetSemanticModelAsync().Result.GetDiagnostics();
        }

        /// <summary>
        /// Returns the string representing the specified document
        /// based on the syntax root.
        /// </summary>
        /// <param name="document">
        /// The <c>>Document</c> to be converted to a string.
        /// </param>
        /// <returns>
        /// A string containing the syntax of the Document after formatting.
        /// </returns>
        private static string GetStringFromDocument(Document document)
        {
            var simplifiedDoc = Simplifier.ReduceAsync(
                document, Simplifier.Annotation).Result;
            var root = simplifiedDoc.GetSyntaxRootAsync().Result;
            root = Formatter.Format(
                root,
                Formatter.Annotation,
                simplifiedDoc.Project.Solution.Workspace);
            return root.GetText().ToString();
        }
    }
}
