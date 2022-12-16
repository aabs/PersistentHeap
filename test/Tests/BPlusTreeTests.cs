namespace PersistentHeap.Tests;
using System;

[TestFixture]
public class BPlusTreeTests
{
    [Test]
    [TestCase(1, 2, 3)]
    [TestCase(1, 3, 2)]
    [TestCase(3, 2, 1)]
    public void CanAdd(params int[] items)
    {
        var sut = new BPlusTree();
        foreach (var i in items)
        {
            sut.Insert(i, 123);
        }
    }

    [Test]
    public void CanAddOne()
    {
        var sut = new BPlusTree();
        sut.Insert(5, 123);
    }

    [Test]
    public void CanCreateANode()
    {
        var sut = new BPlusTree();
        sut.Should().NotBeNull();
    }

    [Test]
    public void OverflowSingleNodeWithRandomNumbers()
    {
        var r = new Random();
        var sut = new BPlusTree();
        for (var i = 0; i < Constants.MaxNodeSize * 2; i++)
        {
            sut.Insert(r.Next(), r.Next());
        }
    }
}
