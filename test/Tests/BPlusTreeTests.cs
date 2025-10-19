#pragma warning disable IDE1006 // Naming Styles
namespace PersistentHeap.Tests;

using System;
using System.Diagnostics;

public class BPlusTreeTests
{
    public record KeyValuePair<TKey, TVal>(TKey Key, TVal Value);

    #region Testing Search Capabilities

    [Fact]
    public void a_tree_with_a_1000_entries_should_have_11_nodes()
    {
        const int degree = 4;
        const int desiredInternalNodes = degree + 1; // two levels of internal nodes
        const int numSamples = (desiredInternalNodes) * degree * degree;
        var sut = new BPlusTree<int, int>();

        for (var i = 0; i < numSamples; i++)
        {
            sut.Insert(i, i);
        }

        var allKeys = sut.AllKeys();
        allKeys.Should().HaveCount(numSamples).And.BeInAscendingOrder();
        sut.AllItems().Should().HaveCount(numSamples).And.BeInAscendingOrder();
        sut.LeafNodes.Count().Should().Be((numSamples / 50) - 1);
        sut.InternalNodes.Count().Should().Be(1);
    }

    [Fact]
    public void a_tree_with_a_large_number_of_entries_can_find_each_element()
    {
        const int numSamples = 50 * Constants.MaxKeysPerNode;
        var sut = new BPlusTree<long, long>();
        for (var i = 0; i < numSamples; i++)
        {
            sut.Insert(i, i * 10);
        }

        for (var i = 0; i < numSamples; i++)
        {
            try
            {
                Assert.Equal(i * 10, sut[i]);
            }
            catch (KeyNotFoundException)
            {
                Assert.Fail();
            }
        }
    }

    [Property]
    public void a_tree_with_many_entries_can_find_each_element(KeyValuePair<int, int>[] testData)
    {
        // must be unique keys
        if (testData.Select(x => x.Key).Distinct().Count() < testData.Length)
        {
            return;
        }

        var sut = new BPlusTree<int, int>();
        foreach (var x in testData)
        {
            sut.Insert(x.Key, x.Value);
        }

        foreach (var x in testData)
        {
            Assert.Equal(x.Value, sut[x.Key]);
        }
    }

    [Property]
    public void a_tree_with_one_entry_can_search_for_the_value(int k, int v)
    {
        var sut = new BPlusTree<int, int>();
        sut.Insert(k, v);
        Assert.Equal(v, sut[k]);
    }

    [Fact]
    public void after_creation_all_node_keys_remain_ordered()
    {
        const int numSamples = 5000;
        var sut = new BPlusTree<int, int>();
        for (var i = 0; i < numSamples; i++)
        {
            sut.Insert(i, i * 10);
        }

        foreach (var n in sut.Nodes)
        {
            var nodeIsOrdered = IsOrdered(n);
            nodeIsOrdered.Should().BeTrue();
        }
    }

    [Property]
    public void all_keys_in_leaf_nodes_are_stored_in_order(KeyValuePair<int, int>[] testData)
    {
        // must be unique keys
        if (testData.Select(x => x.Key).Distinct().Count() < testData.Length)
        {
            return;
        }

        var sut = new BPlusTree<int, int>();
        foreach (var x in testData)
        {
            sut.Insert(x.Key, x.Value);
        }

        var a = sut.AllKeys().ToArray();
        for (var i = 0; i < a.Length - 1; i++)
        {
            a[i].Should().BeLessThan(a[i + 1]);
        }
    }

    [Property]
    public void inserting_duplicate_keys_leaves_last_value_in_tree(int k, int v1, int v2)
    {
        if (v1 == v2)
        {
            return;
        }

        var sut = new BPlusTree<int, int>();
        sut.Insert(k, v1);
        sut.Insert(k, v2);
        Assert.Equal(v2, sut[k]);
    }

    [Property]
    public void searching_an_empty_tree_always_throws_KeyNotFoundException(long x)
    {
        var sut = new BPlusTree<long, long>();
        Assert.Throws<KeyNotFoundException>(() => sut[x]);
    }

    [Fact]
    public void worked_example_from_karlova_lecture_notes()
    {
        int[] xs = [10, 7, 15, 5, 30, 20, 13, 3, 11, 21, 8, 9];
        var sut = new BPlusTree<int, int>(degree: 5);
        foreach (var x in xs)
        {
            sut.Insert(x, x * 7);
        }

        sut.Root.K.Arr.Take(sut.Root.K.Count).Should().BeEquivalentTo(new[] { 10, 15 });
        sut.Root.As<InternalNode<int, int>>().P.Arr.Take(sut.Root.As<InternalNode<int, int>>().P.Count).Should().HaveCount(3);
    }

    private bool IsOrdered(NewNode<int, int> n) => ArrayIsInOrder(n.K.Arr, n.K.Count);
    private bool ArrayIsInOrder(int[] a, int count)
    {
        for (var i = 1; i < count; i++)
        {
            if (a[i] < a[i - 1])
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Testing Splitting Behaviour

    [Fact]
    public void after_split_internal_nodes_remain_ordered()
    {
        const int numSamples = 5000;
        var sut = new BPlusTree<int, int>();
        sut.NodeSplit += (nlo, _) =>
        {
            if (nlo is InternalNode<int, int> nloi)
            {
                var isOrdered = IsOrdered(nloi);
                foreach (var k in nloi.P.Arr[..(nloi.Count + 1)])
                {
                    IsOrdered(k).Should().BeTrue();
                }
            }
        };
        for (var i = 0; i < numSamples; i++)
        {
            sut.Insert(i, i * 10);
        }

    }

    [Fact]
    public void after_split_leaf_nodes_remain_ordered()
    {
        const int numSamples = 5000;
        var sut = new BPlusTree<int, int>();
        sut.NodeSplit += (nlo, _) =>
        {
            // just test the case where a leaf was split
            if (nlo is InternalNode<int, int> intn && intn.Count == 2)
            {
                var isOrdered = IsOrdered(intn);
                IsOrdered(intn.P.Arr[0]).Should().BeTrue();
                IsOrdered(intn.P.Arr[1]).Should().BeTrue();
            }
        };
        for (var i = 0; i < numSamples; i++)
        {
            try
            {
                sut.Insert(i, i * 10);

            }
            catch (ArgumentOutOfRangeException e) when (Debugger.IsAttached)
            {
                _ = e;
                Debugger.Break();
            }

        }

    }

    [Fact]
    public void before_split_internal_nodes_remain_ordered()
    {
        const int numSamples = 5000;
        var sut = new BPlusTree<int, int>();
        sut.NodeSplitting += (n) =>
        {
            if (n is InternalNode<int, int> internalNode)
            {
                var isOrdered = IsOrdered(internalNode);
                foreach (var k in internalNode.P.Arr[..(internalNode.Count + 1)])
                {
                    IsOrdered(k).Should().BeTrue();
                }
            }
        };
        for (var i = 0; i < numSamples; i++)
        {
            sut.Insert(i, i * 10);
        }

    }
    [Fact]
    public void before_split_leaf_nodes_remain_ordered()
    {
        const int numSamples = 5000;
        var sut = new BPlusTree<int, int>();
        sut.NodeSplitting += (n) =>
        {
            if (n is NewLeafNode<int, int> ln)
            {
                IsOrdered(ln).Should().BeTrue();
            }
        };
        for (var i = 0; i < numSamples; i++)
        {
            sut.Insert(i, i * 10);
        }
    }
    #endregion

    [Property]
    public void a_known_key_and_its_associated_data_can_be_removed_from_the_tree(int[] xs)
    {
        // trivial case
        if (xs.Length == 0)
        {
            return;
        }

        var sut = new BPlusTree<long, long>();
        foreach (var x in xs)
        {
            sut.Insert(x, x);
        }

        var valToFind = xs[0];
        sut.ContainsKey(valToFind).Should().BeTrue();
        sut.Delete(valToFind);
        sut.ContainsKey(valToFind).Should().BeFalse();
    }

    [Fact]
    public void a_known_key_and_its_associated_data_can_be_removed_from_the_tree__case_2()
    {
        int[] xs = [-2, -1, 1, 0];

        var sut = new BPlusTree<long, long>();
        foreach (var x in xs)
        {
            sut.Insert(x, x);
        }

        var valToFind = xs[0];
        sut.ContainsKey(valToFind).Should().BeTrue();
        sut.Delete(valToFind);
        sut.ContainsKey(valToFind).Should().BeFalse();
    }

    [Property]
    public void a_known_key_and_its_associated_data_can_be_removed_from_the_tree_case_1()
    {
        int[] xs = [0];
        var sut = new BPlusTree<long, long>();
        foreach (var x in xs)
        {
            sut.Insert(x, x);
        }

        var valToFind = xs[0];
        sut.ContainsKey(valToFind).Should().BeTrue();
        sut.Delete(valToFind);
        sut.ContainsKey(valToFind).Should().BeFalse();
    }
    [Fact]
    public void a_new_tree_starts_with_a_root_node()
    {
        // a new tree has zero child nodes
        var sut = new BPlusTree<long, long>();
        sut.Nodes.Count.Should().Be(1);
        sut.Root.Should().NotBeNull();
    }

    [Fact]
    public void a_new_tree_is_empty_having_only_one_empty_node()
    {
        var sut = new BPlusTree<long, long>();
        sut.Should().NotBeNull();
        sut.Nodes.Count.Should().Be(1);
        sut.Nodes.ElementAt(0).Should().BeSameAs(sut.Root);
        sut.Root.Should().NotBeNull();
    }

    [Property]
    public void adding_a_known_value_into_a_tree_leaves_the_tree_with_the_same_Count_as_before(int[] xs)
    {
        // trivial case
        if (xs.Length == 0)
        {
            return;
        }

        var sut = new BPlusTree<long, long>();
        foreach (var x in xs)
        {
            sut.Insert(x, x);
        }

        var countBefore = sut.Count();
        var newVal = xs[0];
        sut.Insert(newVal, newVal);
        sut.Count().Should().Be(countBefore);
    }

    [Fact]
    public void adding_a_known_value_into_a_tree_leaves_the_tree_with_the_same_Count_as_before__case_1()
    {
        int[] xs = [-1, 2, 0, 3, -2, 1, 4, -3, -4];

        // trivial case
        if (xs.Length == 0)
        {
            return;
        }

        var sut = new BPlusTree<long, long>();
        foreach (var x in xs)
        {
            sut.Insert(x, x);
        }

        var countBefore = sut.Count();
        var newVal = xs[0];
        sut.Insert(newVal, newVal);
        sut.Count().Should().Be(countBefore);
    }

    [Property]
    public void adding_a_new_value_into_a_tree_leaves_the_tree_with_a_Count_one_larger_than_before(int[] xs)
    {
        var sut = new BPlusTree<long, long>();
        foreach (var x in xs)
        {
            sut.Insert(x, x);
        }

        var countBefore = sut.Count();
        var newVal = xs.Length == 0 ? 1 : xs.Max() + 1;
        sut.Insert(newVal, newVal);
        sut.Count().Should().Be(countBefore + 1);
    }

    [Fact]
    public void adding_a_new_value_into_a_tree_leaves_the_tree_with_a_Count_one_larger_than_before__case_1()
    {
        int[] xs = [-2, 6, 3, -1, 2, 1, 4, 5, 0];
        var sut = new BPlusTree<long, long>();
        foreach (var x in xs)
        {
            sut.Insert(x, x);
        }

        var countBefore = sut.Count();
        var newVal = xs.Length == 0 ? 1 : xs.Max() + 1;
        sut.Insert(newVal, newVal);
        sut.Count().Should().Be(countBefore + 1);
    }

    [Fact]
    public void adding_an_element_into_a_tree_with_one_partially_full_node_leaves_the_same_number_of_nodes()
    {
        var sut = new BPlusTree<long, long>();
        for (var i = 0; i < Constants.MaxKeysPerNode - 3; i++)
        {
            sut.Insert(i, i);
        }

        sut.Insert(Constants.MaxKeysPerNode,
            Constants.MaxKeysPerNode); // any other number would do, but we know this isn't in the list
        sut.Nodes.Count.Should().Be(1);
        sut.Root.IsFull.Should().BeFalse();
        sut.InternalNodes.Count().Should().Be(0);
        sut.LeafNodes.Count().Should().Be(1);
    }

    [Property]
    public void adding_an_element_to_an_empty_tree_leave_the_root_node_with_one_element(int i)
    {
        var sut = new BPlusTree<long, long>();
        sut.Insert(i, i);
        sut.Count().Should().Be(1);
    }

    [Fact]
    public void adding_maxnodes_elements_into_a_tree_should_end_with_a_single_node()
    {
        var sut = new BPlusTree<long, long>();
        for (var i = 0; i < Constants.MaxKeysPerNode; i++)
        {
            sut.Insert(i, i);
        }

        sut.Nodes.Count.Should().Be(1);
        sut.Root.IsFull.Should().BeTrue();
        sut.InternalNodes.Count().Should().Be(0);
        sut.LeafNodes.Count().Should().Be(1);
    }

    [Fact]
    public void adding_one_more_element_into_a_tree_with_a_full_node_ends_with_three_nodes()
    {
        var sut = new BPlusTree<long, long>();
        sut.NodeSplitting += (n) =>
        {
            Debugger.Break();
        };
        for (var i = 0; i < Constants.MaxKeysPerNode + 1; i++)
        {
            sut.Insert(i, i);
        }

        sut.Nodes.Count.Should().Be(3);
        sut.Root.IsFull.Should().BeFalse();
        sut.InternalNodes.Count().Should().Be(1);
        sut.LeafNodes.Count().Should().Be(2);
    }
    [Fact]
    public void can_add_any_integer_to_tree() =>
        Prop.ForAll<int>(i =>
            {
                var sut = new BPlusTree<long, long>();
                sut.Insert(i, i);

                return sut.Count() == 1;
            })
            .QuickCheckThrowOnFailure();

    [Property]
    public void can_add_any_number_of_items_to_tree(int[] xs)
    {
        var sut = new BPlusTree<long, long>();
        foreach (var i in xs)
        {
            sut.Insert(i, i);
        }

        sut.Count().Should().Be(xs.Distinct().Count());
    }

    [Fact]
    public void can_add_any_number_of_items_to_tree__case_1()
    {
        int[] xs = [4, 0, 5, -1, 3, -2, -4, 2, 1, -3, 1];
        var sut = new BPlusTree<long, long>();
        foreach (var i in xs)
        {
            sut.Insert(i, i);
        }

        sut.Count().Should().Be(xs.Distinct().Count());
    }

    [Fact]
    public void can_create_a_node()
    {
        var sut = new BPlusTree<long, long>();
        sut.Should().NotBeNull();
    }

    [Fact]
    public void inserting_MaxNodeSize_minus_1_items_into_a_tree_leaves_a_tree_with_one_node_that_is_not_full()
    {
        var sut = new BPlusTree<long, long>();
        for (var i = 0; i < Constants.MaxKeysPerNode - 1; i++)
        {
            sut.Insert(i, i);
        }

        // assert that there is only one full node
        sut.Nodes.Count.Should().Be(1);
        sut.Root.IsFull.Should().BeFalse(); // adding one more item to the node would trigger a split.
        sut.Root.Count.Should().Be(Constants.MaxKeysPerNode - 1);
    }

    // removing a value from the tree reduces its count by 1
    [Property]
    public void removing_a_value_from_the_tree_reduces_its_count_by_1(int[] xs)
    {
        // trivial case
        if (xs.Length == 0)
        {
            return;
        }

        var sut = new BPlusTree<long, long>();
        foreach (var x in xs)
        {
            sut.Insert(x, x);
        }

        var countBefore = sut.Count();
        var valToRemove = xs[0];
        var w = sut.Delete(valToRemove);
        w.Should().NotBeNull();
        w!.Value.Item1.Should().Be(valToRemove);
        sut.Count().Should().Be(countBefore - 1);
    }

    [Fact]
    public void removing_a_value_from_the_tree_reduces_its_count_by_1__case_1()
    {
        int[] xs = [-2, -3, 5, 0, 4, 2, 3, -1, 1];

        var sut = new BPlusTree<long, long>();
        foreach (var x in xs)
        {
            sut.Insert(x, x);
        }

        var countBefore = sut.Count();
        var valToRemove = xs[0];
        var w = sut.Delete(valToRemove);
        w.Should().NotBeNull();
        w!.Value.Item1.Should().Be(valToRemove);
        sut.Count().Should().Be(countBefore - 1);
    }

    [Fact]
    public void removing_a_value_from_the_tree_reduces_its_count_by_1__case_2()
    {
        int[] xs = [5, -3, 1, 2, -4, 3, 4, 0, -2, -1];

        var sut = new BPlusTree<long, long>();
        foreach (var x in xs)
        {
            sut.Insert(x, x);
        }

        var countBefore = sut.Count();
        var valToRemove = xs[0];
        var w = sut.Delete(valToRemove);
        w.Should().NotBeNull();
        w!.Value.Item1.Should().Be(valToRemove);
        sut.Count().Should().Be(countBefore - 1);
    }

    [Fact]
    public void removing_a_value_from_the_tree_reduces_its_count_by_1__case_3()
    {
        int[] xs = [-2, 5, -4, 4, 3, -1, 1, 2, -3, 0, 0];

        var sut = new BPlusTree<long, long>();
        foreach (var x in xs)
        {
            sut.Insert(x, x);
        }

        var countBefore = sut.Count();
        var valToRemove = xs[0];
        var w = sut.Delete(valToRemove);
        w.Should().NotBeNull();
        w!.Value.Item1.Should().Be(valToRemove);
        sut.Count().Should().Be(countBefore - 1);
    }

    [Property]
    public void removing_an_unknown_value_from_a_tree_results_in_an_unknown_key_exception(int[] xs)
    {
        // trivial case
        if (xs.Length == 0)
        {
            return;
        }

        var sut = new BPlusTree<long, long>();
        foreach (var x in xs)
        {
            sut.Insert(x, x);
        }

        var newVal = xs.Length == 0 ? 1 : xs.Max() + 1;
        Assert.Throws<KeyNotFoundException>(() => sut.Delete(newVal));
    }

    [Property]
    public void searching_for_a_key_returns_the_same_value_as_was_inserted(Dictionary<int, string> testData)
    {
        // trivial case
        if (testData.Count == 0)
        {
            return;
        }

        var sut = new BPlusTree<int, string>();
        foreach (var x in testData.Keys)
        {
            sut.Insert(x, testData[x]);
        }

        var valToFind = testData.Keys.ElementAt(0);
        var foundValue = sut[valToFind];

        //foundValue.Should().NotBeNull();
        Assert.Equal(foundValue, testData[valToFind]);
    }
    [Fact]
    public void there_should_be_no_overlap_between_the_key_ranges_of_internal_nodes()
    {
        var sut = new BPlusTree<int, int>();
        var keyCtr = 0;

        var splitCounter = 0;
        sut.NodeSplitting += (n) =>
        {
            splitCounter++;
            if (n is InternalNode<int, int>)
            {
                Debugger.Break();
            }
        };
        while (splitCounter < 5)
        {
            var x = keyCtr++;
            sut.Insert(x, x);
        }
        Debug.WriteLine(new BPlusTreeRenderer<int, int>().Render(sut));
        // now check that none of the internal nodes has overlapping Key Ranges
        var keys = sut.InternalNodes.Select(n => (n.K.Arr[0], n.K.Arr[n.K.Count - 1])).OrderBy(x => x.Item1).ToArray();
        for (var i = 0; i < keys.Length - 1; i++)
        {
            var isLess = keys[i].Item2 < keys[i + 1].Item1;
            isLess.Should().BeTrue();
        }

    }
}

#pragma warning restore IDE1006 // Naming Styles
