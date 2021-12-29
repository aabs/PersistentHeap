using System;

namespace PersistentHeap.Tests;

[TestClass]
public class BPlusTreeTests
{
    [TestMethod]
    [DataRow(1, 2, 3)]
    [DataRow(1, 3, 2)]
    [DataRow(3, 2, 1)]
    public void CanAdd(params int[] items)
    {
        var sut = new BPlusTree();
        foreach (var i in items)
        {
            sut.Insert(i, 123);
        }
    }

    [TestMethod]
    public void CanAddOne()
    {
        var sut = new BPlusTree();
        sut.Insert(5, 123);
    }

    [TestMethod]
    public void CanCreateANode()
    {
        var sut = new BPlusTree();
        sut.Should().NotBeNull();
    }

    [TestMethod]
    public void OverflowSingleNodeWithRandomNumbers()
    {
        var r = new Random();
        var sut = new BPlusTree();
        for (int i = 0; i < Constants.MaxNodeSize * 2; i++)
        {
            sut.Insert(r.Next(), r.Next());
        }
    }
}