using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace StyleChecker.Test
{
    [TestClass]
    public class LongLineAnalyzerTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public int alpha = 1;
            public int                                                         beta = 2;
/*
        8      16      24      32      40      48      56      64      72
        |       |       |       |       |       |       |       |       |
*/
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "LongLineAnalyzer",
                Message = "This line is too long.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 89)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new LongLineAnalyzer();
        }
    }
}
