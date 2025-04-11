namespace TestSuite.Refactoring.IsNull;

using Analyzers.Refactoring.IsNull;
using BeliefCrucible;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Refactoring.IsNull;

[TestClass]
public sealed class EqualNullCodeFixTest : CodeFixVerifier
{
    public EqualNullCodeFixTest()
        : base(new Analyzer(), new EqualNullCodeFixer())
    {
    }

    [TestMethod]
    public void Code()
    {
        var code = ReadText("Code");
        var fix = ReadText("EqualNullCodeFix");
        static Result Expected(Belief b)
        {
            var token = b.Message;
            return b.ToResult(
                Analyzer.DiagnosticId,
                $"Use '{token}' operator instead of 'is' pattern matching.",
                DiagnosticSeverity.Info);
        }

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
