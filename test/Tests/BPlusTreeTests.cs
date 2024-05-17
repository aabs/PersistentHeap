#pragma warning disable IDE1006 // Naming Styles
namespace PersistentHeap.Tests;

public class BPlusTreeTests
{
    public record KeyValuePair<TKey, TVal>(TKey Key, TVal Value);

    #region Testing Search Capabilities

    [Fact]
    public void a_tree_with_a_large_number_of_entries_can_find_each_element()
    {
        var sut = new BPlusTree<long, long>();
        for (var i = 0; i < 100000; i++)
        {
            sut.Insert(i, i * 10);
        }

        for (var i = 0; i < 100000; i++)
        {
            Assert.Equal(i * 10, sut[i]);
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
        var w = sut.Delete(valToFind);
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
        var w = sut.Delete(valToFind);
        sut.ContainsKey(valToFind).Should().BeFalse();
    }

    [Fact]
    public void a_new_tree_has_zero_child_nodes()
    {
        // a new tree has zero child nodes
        var sut = new BPlusTree<long, long>();
        sut.Nodes.Count.Should().Be(1);
        sut.RootIndexNode.Should().Be(0);
    }

    [Fact]
    public void a_new_tree_is_empty_having_only_one_empty_node()
    {
        var sut = new BPlusTree<long, long>();
        sut.Should().NotBeNull();
        sut.Nodes.Count.Should().Be(1);
        sut.Nodes.ElementAt(0).Should().BeSameAs(sut.Root);
        sut.RootIndexNode.Should().Be(0);
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
        for (var i = 0; i < Constants.MaxNodeSize - 3; i++)
        {
            sut.Insert(i, i);
        }

        sut.Insert(Constants.MaxNodeSize,
            Constants.MaxNodeSize); // any other number would do, but we know this isn't in the list
        sut.Nodes.Count.Should().Be(1);
        sut.Root.IsFull.Should().BeFalse();
        sut.Nodes.Count(n => n.IsDeleted).Should().Be(0);
        sut.Nodes.Count(n => !n.IsDeleted).Should().Be(1);
    }

    [Property]
    public void adding_an_element_to_an_empty_tree_leave_the_root_node_with_one_element(int i)
    {
        var sut = new BPlusTree<long, long>();
        sut.Insert(i, i);
        sut.Count().Should().Be(1);
    }

    [Fact]
    public void
        adding_one_element_into_a_tree_with_one_full_node_leaves_a_tree_with_three_live_nodes_and_one_deleted_node()
    {
        var sut = new BPlusTree<long, long>();
        for (var i = 0; i < Constants.MaxNodeSize; i++)
        {
            sut.Insert(i, i);
        }

        sut.Nodes.Count.Should().Be(3);
        sut.Root.IsFull.Should().BeFalse();
        sut.Nodes.Count(n => n.IsDeleted).Should().Be(0);
        sut.Nodes.Count(n => !n.IsDeleted).Should().Be(3);
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

    [Property]
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
        for (var i = 0; i < Constants.MaxNodeSize - 1; i++)
        {
            sut.Insert(i, i);
        }

        // assert that there is only one full node
        sut.Nodes.Count.Should().Be(1);
        sut.Root.IsFull.Should().BeFalse(); // adding one more item to the node would trigger a split.
        sut.Root.KeysInUse.Should().Be(Constants.MaxNodeSize - 1);
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
        w.Value.Item1.Should().Be(valToRemove);
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
        w.Value.Item1.Should().Be(valToRemove);
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
        w.Value.Item1.Should().Be(valToRemove);
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
        w.Value.Item1.Should().Be(valToRemove);
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
}
#pragma warning restore IDE1006 // Naming Styles
