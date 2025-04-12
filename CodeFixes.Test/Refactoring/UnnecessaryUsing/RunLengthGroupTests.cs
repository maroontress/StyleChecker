namespace CodeFixes.Test.Refactoring.UnnecessaryUsing;

using System;
using System.Collections.Generic;
using System.Linq;
using CodeFixes.Refactoring.UnnecessaryUsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class RunLengthGroupTests
{
    [TestMethod]
    public void EmptySequenceReturnsEmptyGroups()
    {
        int[] source = [];
        var result = source.RunLengthGroupBy(x => x);
        Assert.IsFalse(result.Any());
    }

    [TestMethod]
    public void SingleElementSequenceReturnsSingleGroup()
    {
        List<int> source = [1];
        var result = source.RunLengthGroupBy(x => x);
        Assert.AreEqual(1, result.Count());
        var first = result.First();
        Assert.AreEqual(1, first.Key);
        CollectionAssert.AreEqual(source, first.ToList());
    }

    [TestMethod]
    public void SequenceWithSameElementsReturnsSingleGroup()
    {
        List<int> source = [1, 1, 1, 1];
        var result = source.RunLengthGroupBy(x => x);
        Assert.AreEqual(1, result.Count());
        var first = result.First();
        Assert.AreEqual(1, first.Key);
        CollectionAssert.AreEqual(source, first.ToList());
    }

    [TestMethod]
    public void SequenceWithDifferentElementsReturnsMultipleGroups()
    {
        List<int> source = [1, 2, 2, 3, 3, 3];
        var result = source.RunLengthGroupBy(x => x);
        Assert.AreEqual(3, result.Count());
        var actualList = result.ToList();
        Assert.AreEqual(1, actualList[0].Key);
        Assert.AreEqual(2, actualList[1].Key);
        Assert.AreEqual(3, actualList[2].Key);
        List<List<int>> expected = [[1], [2, 2], [3, 3, 3]];
        for (var k = 0; k < 3; ++k)
        {
            CollectionAssert.AreEqual(expected[k], actualList[k].ToList());
        }
    }

    [TestMethod]
    public void Basic()
    {
        List<int> source = [
            1, 1, 1,
            2, 2,
            3, 3, 3,
            2,
            3, 3,
            1, 1,
        ];
        var result = source.RunLengthGroupBy(x => x);
        Assert.AreEqual(6, result.Count());
        var actualList = result.ToList();
        List<int> expectedKeys = [1, 2, 3, 2, 3, 1];
        for (var k = 0; k < 6; ++k)
        {
            Assert.AreEqual(expectedKeys[k], actualList[k].Key);
        }
        List<List<int>> expected = [
            [1, 1, 1],
            [2, 2],
            [3, 3, 3],
            [2],
            [3, 3],
            [1, 1],
        ];
        for (var k = 0; k < 6; ++k)
        {
            CollectionAssert.AreEqual(expected[k], actualList[k].ToList());
        }
    }
}
