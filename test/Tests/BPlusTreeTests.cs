namespace PersistentHeap.Tests;
using System;
using DotNext.Text;



public class BPlusTreeTests
{
    [Fact]
    public void a_new_tree_is_empty_having_only_one_empty_node()
    {
        var sut = new BPlusTree();
        sut.Should().NotBeNull();
        sut.Nodes.Count.Should().Be(1);
        sut.Nodes.ElementAt(0).Should().BeSameAs(sut.Root);
        sut.RootIndexNode.Should().Be(0);
    }

    [Fact]
    public void a_new_tree_has_zero_child_nodes()
    {
        // a new tree has zero child nodes
        var sut = new BPlusTree();
        sut.Nodes.Count.Should().Be(1);
        sut.RootIndexNode.Should().Be(0);
    }

     [Property]
    public void adding_an_element_to_an_empty_tree_leave_the_root_node_with_one_element(int i)
    {
        var sut = new BPlusTree();
        sut.Insert(i, i);
        sut.Count().Should().Be(1);
    }

    [Property]
    public void can_add_any_number_of_items_to_tree(int[] xs)
    {
        var sut = new BPlusTree();
        foreach (var i in xs)
        {
            sut.Insert(i, i);
        }

        sut.Count().Should().Be(xs.Distinct().Count());
    }

   [Fact]
    public void inserting_MaxNodeSize_minus_1_items_into_a_tree_leaves_a_tree_with_one_node_that_is_not_full()
    {
        var sut = new BPlusTree();
        for (var i = 0; i < Constants.MaxNodeSize-1; i++)
        {
            sut.Insert(i, i);
        }
        // assert that there is only one full node
        sut.Nodes.Count.Should().Be(1);
        sut.Root.IsFull.Should().BeFalse(); // adding one more item to the node would trigger a split.
        sut.Root.KeysInUse.Should().Be(Constants.MaxNodeSize-1);
    }

    // adding one element into a tree with one full node leaves a tree with three live nodes and one deleted node
    // adding an element into a tree with one partially full node, leaves the same number of nodes
    // adding a new value into a tree leaves the tree with a Count one larger than before
    // a tree can accept a duplicate key without exception
    // a known key and its associated data can be removed from the tree
    // removing a value from the tree reduces its count by 1
    // removing an unknown value from a tree results in an unknown key exception


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
}
