namespace Roastery.Test;

using System.Linq;
using Maroontress.Roastery;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class EnumerableExtensionsTest
{
    [TestMethod]
    public void Separate_EmptySequence_ReturnsEmptySequence()
    {
        var input = Enumerable.Empty<int>();
        var result = input.Separate(0);
        Assert.IsFalse(result.Any());
    }

    [TestMethod]
    public void Separate_SingleElement_ReturnsSingleElement()
    {
        List<int> input = [42];
        var result = input.Separate(0);
        CollectionAssert.AreEqual(input, result.ToList());
    }

    [TestMethod]
    public void Separate_MultipleElements_InsertsSpacerBetweenElements()
    {
        List<int> input = [1, 2, 3];
        List<int> expected = [1, 0, 2, 0, 3];
        var result = input.Separate(0);
        CollectionAssert.AreEqual(expected, result.ToList());
    }

    [TestMethod]
    public void Separate_StringSequence_InsertsDelimiter()
    {
        List<string> input = ["a", "b", "c"];
        List<string> expected = ["a", "|", "b", "|", "c"];
        var result = input.Separate("|");
        CollectionAssert.AreEqual(expected, result.ToList());
    }
}
