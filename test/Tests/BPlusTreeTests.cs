namespace PersistentHeap.Tests;
using System;
using DotNext.Text;



public class BPlusTreeTests
{
    [Fact]
    public void can_add_any_number_of_items_to_tree()
    {
        Prop.ForAll<int[]>(xs =>
            {
                var sut = new BPlusTree();
                foreach (var i in xs)
                {
                    sut.Insert(i, 123);
                }

                var result = sut.Count() == xs.Distinct().Count();
                return result;
            })
            .QuickCheckThrowOnFailure();
    }

    [Fact]
    public void can_add_any_integer_to_tree()
    {
        Prop.ForAll<int>(i =>
            {
                var sut = new BPlusTree();
                sut.Insert(i, i);
                return sut.Count() == 1;
            })
            .QuickCheckThrowOnFailure();
    }

    [Fact]
    public void can_create_a_node()
    {
        var sut = new BPlusTree();
        sut.Should().NotBeNull();
    }

    [Fact]
    public void OverflowSingleNodeWithRandomNumbers()
    {
        Assert.Throws<OverfullNodeException>(() =>
        {
            var sut = new BPlusTree();
            var r = new Random();
            for (var i = 0; i < Constants.MaxNodeSize * 2; i++)
            {
                sut.Insert(r.Next(), i);
            }
        });

    }
}
