using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace StyleChecker.Test
{
    [TestClass]
    public class UnderscoreAnalyzerTest : CodeFixVerifier
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
            public int beta = 2;
            public void gamma()
            {
                int _delta = 4;
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "UnderscoreAnalyzer",
                Message = string.Format("The name '{0}' includes a underscore.", "_delta"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 17, 21)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
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
            public int beta = 2;
            public void gamma()
            {
                int delta = 4;
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod3()
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
            public int beta = 2;
            public void gamma()
            {
                int foo_bar = 4;
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "UnderscoreAnalyzer",
                Message = string.Format("The name '{0}' includes a underscore.", "foo_bar"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 17, 21)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
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
            public int beta = 2;
            public void gamma()
            {
                int fooBar = 4;
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new UnderscoreCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UnderscoreAnalyzer();
        }
    }
}
