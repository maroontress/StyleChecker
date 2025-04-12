namespace Roastery.Test;

using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roastery;

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
        var actual = TextTemplates.Substitute(template, toValue);
        Assert.AreEqual("value", actual);
    }

    [TestMethod]
    public void Substitute_ShouldIgnoreNonKeyPatterns()
    {
        var map = new Dictionary<string, string>();
        var toValue = (string s) => map[s];
        var template = "$key";
        var actual = TextTemplates.Substitute(template, toValue);
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
        var actual = TextTemplates.Substitute(template, toValue);
        Assert.AreEqual("value1 and value2", actual);
    }

    [TestMethod]
    public void Substitute_ShouldHandleEmptyTemplate()
    {
        var map = new Dictionary<string, string>();
        var toValue = (string s) => map[s];
        var template = "";
        var actual = TextTemplates.Substitute(template, toValue);
        Assert.AreEqual(template, actual);
    }

    [TestMethod]
    public void Substitute_ShouldThrowEndOfStreamException()
    {
        var map = new Dictionary<string, string>();
        var toValue = (string s) => map[s];
        var template = "${key";
        Assert.ThrowsExactly<EndOfStreamException>(
            () => _ = TextTemplates.Substitute(template, toValue));
    }
}
