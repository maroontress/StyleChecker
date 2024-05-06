namespace StyleChecker.Test.Framework;

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class BeliefsTests
{
    [TestMethod]
    public void Decode_ComplileError()
    {
        var encodedSource = """
            namespace StyleChecker.Test.Framework;

            public sealed class Hello
            {
                public void Print()
                {
                    Console.WriteLine("Hello, world!");
                }
            }
            """;
        ExpectCompilationException(encodedSource);
    }

    [TestMethod]
    public void Decode_NoBeliefs()
    {
        var encodedSource = """
            namespace StyleChecker.Test.Framework;

            using System;

            public sealed class Hello
            {
                public void Print()
                {
                    Console.WriteLine("Hello, world!");
                }
            }
            """;
        ExpectCompilationException(encodedSource);
    }

    [TestMethod]
    public void Decode_InvalidSyntax()
    {
        var encodedSource = """
            namespace StyleChecker.Test.Framework;
            
            using System;
            
            public sealed class Hello
            {
                public void Print()
                //@
                {
                    Console.WriteLine("Hello, world!");
                }
            }
            """;
        ExpectCompilationException(encodedSource);
    }

    [TestMethod]
    public void Decode_InvalidChar()
    {
        var encodedSource = """
            namespace StyleChecker.Test.Framework;
            
            using System;
            
            public sealed class Hello
            {
                public void Print()
            //@3
                {
                    Console.WriteLine("Hello, world!");
                }
            }
            """;
        ExpectCompilationException(encodedSource);
    }

    [TestMethod]
    public void Decode_NoCircumflex()
    {
        var encodedSource = """
            namespace StyleChecker.Test.Framework;
            
            using System;
            
            public sealed class Hello
            {
                public void Print()
            //@ hello world!
                {
                    Console.WriteLine("Hello, world!");
                }
            }
            """;
        ExpectCompilationException(encodedSource);
    }

    [TestMethod]
    public void Decode_NoLocationMatched()
    {
        var encodedSource = """
            namespace StyleChecker.Test.Framework;
            
            using System;
            
            public sealed class Hello
            {
                public void Print()
            //@   ^public
                {
                    Console.WriteLine("Hello, world!");
                }
            }
            """;
        ExpectCompilationException(encodedSource);
    }

    [TestMethod]
    public void Decode_SingleBelief()
    {
        var encodedSource = """
            namespace StyleChecker.Test.Framework;

            using System;

            public sealed class Hello
            {
                public void print()
                //@         ^Print
                {
                    Console.WriteLine("Hello, world!");
                }
            }
            """;
        var decodedSource = """
            namespace StyleChecker.Test.Framework;

            using System;

            public sealed class Hello
            {
                public void print()
                {
                    Console.WriteLine("Hello, world!");
                }
            }
            """;
        var excludeIds = Array.Empty<string>();
        static Result ToResult(Belief b)
            => b.ToResult("TestId", b.Message);

        var (actualSource, actualResults)
            = Beliefs.Decode(encodedSource, excludeIds, ToResult);
        Assert.AreEqual(decodedSource, actualSource);
        var resultList = actualResults.ToList();
        Assert.AreEqual(1, resultList.Count);
        var firstResult = resultList[0];
        Assert.AreEqual("TestId", firstResult.Id);
        Assert.AreEqual("Print", firstResult.Message);
        Assert.AreEqual(17, firstResult.Column);
        Assert.AreEqual(7, firstResult.Line);
        Assert.AreEqual(DiagnosticSeverity.Warning, firstResult.Severity);
        Assert.AreEqual(1, firstResult.Locations.Length);
        Assert.AreEqual(17, firstResult.Locations[0].Column);
        Assert.AreEqual(7, firstResult.Locations[0].Line);
    }

    [TestMethod]
    public void Decode_DoubleBelief()
    {
        var encodedSource = """
            namespace StyleChecker.Test.Framework;

            using System;

            public sealed class Hello
            //@0Public
            {
                public void print()
                //@         ^Print
                {
                    Console.WriteLine("Hello, world!");
                }
            }
            """;
        var decodedSource = """
            namespace StyleChecker.Test.Framework;

            using System;

            public sealed class Hello
            {
                public void print()
                {
                    Console.WriteLine("Hello, world!");
                }
            }
            """;
        var excludeIds = Array.Empty<string>();
        static Result ToResult(Belief b)
            => b.ToResult("TestId", b.Message);

        var (actualSource, actualResults)
            = Beliefs.Decode(encodedSource, excludeIds, ToResult);
        Assert.AreEqual(decodedSource, actualSource);
        var resultList = actualResults.ToList();
        Assert.AreEqual(2, resultList.Count);
        {
            var firstResult = resultList[0];
            Assert.AreEqual("TestId", firstResult.Id);
            Assert.AreEqual("Public", firstResult.Message);
            Assert.AreEqual(1, firstResult.Column);
            Assert.AreEqual(5, firstResult.Line);
            Assert.AreEqual(DiagnosticSeverity.Warning, firstResult.Severity);
            Assert.AreEqual(1, firstResult.Locations.Length);
            Assert.AreEqual(1, firstResult.Locations[0].Column);
            Assert.AreEqual(5, firstResult.Locations[0].Line);
        }
        {
            var secondResult = resultList[1];
            Assert.AreEqual("TestId", secondResult.Id);
            Assert.AreEqual("Print", secondResult.Message);
            Assert.AreEqual(17, secondResult.Column);
            Assert.AreEqual(7, secondResult.Line);
            Assert.AreEqual(DiagnosticSeverity.Warning, secondResult.Severity);
            Assert.AreEqual(1, secondResult.Locations.Length);
            Assert.AreEqual(17, secondResult.Locations[0].Column);
            Assert.AreEqual(7, secondResult.Locations[0].Line);
        }
    }

    [TestMethod]
    public void Decode_OneAndTwoBeliefs()
    {
        var encodedSource = """
            namespace StyleChecker.Test.Framework;

             public record class Foo(int Bar)
            //@1public
              {
            //@2{
            }
            """;
        var decodedSource = """
            namespace StyleChecker.Test.Framework;
            
             public record class Foo(int Bar)
              {
            }
            """;
        var excludeIds = Array.Empty<string>();
        static Result ToResult(Belief b)
            => b.ToResult("TestId", b.Message);

        var (actualSource, actualResults)
            = Beliefs.Decode(encodedSource, excludeIds, ToResult);
        Assert.AreEqual(decodedSource, actualSource);
        var resultList = actualResults.ToList();
        Assert.AreEqual(2, resultList.Count);
        {
            var firstResult = resultList[0];
            Assert.AreEqual("TestId", firstResult.Id);
            Assert.AreEqual("public", firstResult.Message);
            Assert.AreEqual(2, firstResult.Column);
            Assert.AreEqual(3, firstResult.Line);
            Assert.AreEqual(DiagnosticSeverity.Warning, firstResult.Severity);
            Assert.AreEqual(1, firstResult.Locations.Length);
            Assert.AreEqual(2, firstResult.Locations[0].Column);
            Assert.AreEqual(3, firstResult.Locations[0].Line);
        }
        {
            var secondResult = resultList[1];
            Assert.AreEqual("TestId", secondResult.Id);
            Assert.AreEqual("{", secondResult.Message);
            Assert.AreEqual(3, secondResult.Column);
            Assert.AreEqual(4, secondResult.Line);
            Assert.AreEqual(DiagnosticSeverity.Warning, secondResult.Severity);
            Assert.AreEqual(1, secondResult.Locations.Length);
            Assert.AreEqual(3, secondResult.Locations[0].Column);
            Assert.AreEqual(4, secondResult.Locations[0].Line);
        }
    }

    [TestMethod]
    public void Decode_ShiftUpward()
    {
        var encodedSource = """
            namespace StyleChecker.Test.Framework;
            
            /**
             * <summary>
             * </summary>
             */
            //@ ^^^summary
            public record class Foo(int Bar)
            {
            }
            """;
        var decodedSource = """
            namespace StyleChecker.Test.Framework;
            
            /**
             * <summary>
             * </summary>
             */
            public record class Foo(int Bar)
            {
            }
            """;
        var excludeIds = Array.Empty<string>();
        static Result ToResult(Belief b)
            => b.ToResult("TestId", b.Message);

        var (actualSource, actualResults)
            = Beliefs.Decode(encodedSource, excludeIds, ToResult);
        Assert.AreEqual(decodedSource, actualSource);
        var resultList = actualResults.ToList();
        Assert.AreEqual(1, resultList.Count);
        var firstResult = resultList[0];
        Assert.AreEqual("TestId", firstResult.Id);
        Assert.AreEqual("summary", firstResult.Message);
        Assert.AreEqual(5, firstResult.Column);
        Assert.AreEqual(4, firstResult.Line);
        Assert.AreEqual(DiagnosticSeverity.Warning, firstResult.Severity);
        Assert.AreEqual(1, firstResult.Locations.Length);
        Assert.AreEqual(5, firstResult.Locations[0].Column);
        Assert.AreEqual(4, firstResult.Locations[0].Line);
    }

    [TestMethod]
    public void Decode_ShiftUpward_Zero()
    {
        var encodedSource = """
            namespace StyleChecker.Test.Framework;
            
            /**
             * <summary>
             * </summary>
             */
            //@0^^^/**
            public record class Foo(int Bar)
            {
            }
            """;
        var decodedSource = """
            namespace StyleChecker.Test.Framework;
            
            /**
             * <summary>
             * </summary>
             */
            public record class Foo(int Bar)
            {
            }
            """;
        var excludeIds = Array.Empty<string>();
        static Result ToResult(Belief b)
            => b.ToResult("TestId", b.Message);

        var (actualSource, actualResults)
            = Beliefs.Decode(encodedSource, excludeIds, ToResult);
        Assert.AreEqual(decodedSource, actualSource);
        var resultList = actualResults.ToList();
        Assert.AreEqual(1, resultList.Count);
        var firstResult = resultList[0];
        Assert.AreEqual("TestId", firstResult.Id);
        Assert.AreEqual("/**", firstResult.Message);
        Assert.AreEqual(1, firstResult.Column);
        Assert.AreEqual(3, firstResult.Line);
        Assert.AreEqual(DiagnosticSeverity.Warning, firstResult.Severity);
        Assert.AreEqual(1, firstResult.Locations.Length);
        Assert.AreEqual(1, firstResult.Locations[0].Column);
        Assert.AreEqual(3, firstResult.Locations[0].Line);
    }

    private void ExpectCompilationException(string encodedSource)
    {
        static Result ToResult(Belief b)
            => b.ToResult("TestId", b.Message);

        var excludeIds = Array.Empty<string>();
        Assert.ThrowsException<CompilationException>(
            () => _ = Beliefs.Decode(encodedSource, excludeIds, ToResult));
    }
}
