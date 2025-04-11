namespace Analyzers.Test.Refactoring;

using System.Collections.Generic;
using System.IO;
using Analyzers.Refactoring;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class TextsTest
{
    [TestMethod]
    public void Substitute_ShouldReplaceKeyWithMappedValue()
    {
        var map = new Dictionary<string, string>
        {
            ["key"] = "value",
        };
        var toValue = (string s) => map[s];
        var template = "${key}";
        var actual = Texts.Substitute(template, toValue);
        Assert.AreEqual("value", actual);
    }

    [TestMethod]
    public void Substitute_ShouldIgnoreNonKeyPatterns()
    {
        var map = new Dictionary<string, string>();
        var toValue = (string s) => map[s];
        var template = "$key";
        var actual = Texts.Substitute(template, toValue);
        Assert.AreEqual(template, actual);
    }

    [TestMethod]
    public void Substitute_ShouldHandleMultipleKeys()
    {
        var map = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
        };
        var toValue = (string s) => map[s];
        var template = "${key1} and ${key2}";
        var actual = Texts.Substitute(template, toValue);
        Assert.AreEqual("value1 and value2", actual);
    }

    [TestMethod]
    public void Substitute_ShouldHandleEmptyTemplate()
    {
        var map = new Dictionary<string, string>();
        var toValue = (string s) => map[s];
        var template = "";
        var actual = Texts.Substitute(template, toValue);
        Assert.AreEqual(template, actual);
    }

    [TestMethod]
    public void Substitute_ShouldThrowEndOfStreamException()
    {
        var map = new Dictionary<string, string>();
        var toValue = (string s) => map[s];
        var template = "${key";
        Assert.ThrowsExactly<EndOfStreamException>(
            () => _ = Texts.Substitute(template, toValue));
    }
}
