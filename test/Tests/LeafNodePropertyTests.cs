namespace PersistentHeap.Tests;

using DotNext;
using Random = System.Random;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class LeafNodePropertyTests
{
    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    [Trait("Category", "Property")]
    public void adding_a_key_to_a_node_that_already_contains_it_does_not_add_anything(int[] xs)
    {
        var sut = new NewLeafNode<long, long>(Constants.MaxKeysPerNode);
        foreach (var i in xs)
        {
            sut.Insert(i, 123);
        }

        // now let's try to insert one of the already inserted keys, and see what happens
        var expected = sut.Count;
        sut.Insert(xs[0], 123);
        var actual = sut.Count;
        actual.Should().Be(expected);
    }

    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    [Trait("Category", "Property")]
    public void adding_a_key_to_a_node_that_already_contains_it_does_not_add_anything__case1()
    {
        int[] xs = [3, 2, 4];
        var sut = new NewLeafNode<long, long>(Constants.MaxKeysPerNode);
        foreach (var i in xs)
        {
            sut.Insert(i, 123);
        }

        // now let's try to insert one of the already inserted keys, and see what happens
        var expected = sut.Count;
        sut.Insert(xs[0], 123);
        var actual = sut.Count;
        actual.Should().Be(expected);
    }
    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    [Trait("Category", "Property")]
    public void adding_a_new_key_to_a_non_full_node_increases_keys_by_one(int[] xs)
    {
        if (xs.Length > Constants.MaxKeysPerNode)
        {
            return;
        }
        var sut = new NewLeafNode<long, long>(Constants.MaxKeysPerNode);
        foreach (var i in xs)
        {
            sut.Insert(i, 123, overwriteOnEquality: true);
        }

        sut.Count.Should().Be(xs.Distinct().Count());
    }

    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    [Trait("Category", "Property")]
    public void adding_keys_to_a_node_leaves_the_node_keys_in_order(int[] xs)
    {
        var sut = new NewLeafNode<long, long>(Constants.MaxKeysPerNode);
        foreach (var i in xs)
        {
            sut.Insert(i, 123, overwriteOnEquality: true);
        }

        var expected = xs.Distinct().OrderBy(x => x).Select(i => (long)i).ToArray();
        var actual = sut.K.Arr[..sut.Count];
        expected.Should().BeEquivalentTo(actual);
    }

    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    [Trait("Category", "Property")]
    public void removing_a_key_from_a_node_reduces_the_number_of_keys_by_one(int[] xs)
    {
        var sut = new NewLeafNode<long, long>(Constants.MaxKeysPerNode);
        foreach (var i in xs)
        {
            sut.Insert(i, 123, overwriteOnEquality: true);
        }

        var expected = sut.Count - 1;
        sut.Delete(xs[0]);
        var actual = sut.Count;
        expected.Should().Be(actual);
    }

    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    [Trait("Category", "Property")]
    public void removing_a_key_from_a_node_leaves_the_node_keys_in_order(int[] xs)
    {
        var sut = new NewLeafNode<long, long>(Constants.MaxKeysPerNode);
        foreach (var i in xs)
        {
            sut.Insert(i, 123, overwriteOnEquality: true);
        }

        if (xs.Length == 0 || sut.Count == 0)
        {
            return;
        }
        sut.Delete(xs[0]);

        ArrayIsInOrder(sut.K.Arr[..sut.Count]).Should().BeTrue();
    }

    [Fact]
    public void removing_an_element_from_an_empty_node_changes_nothing()
    {
        var sut = new NewLeafNode<long, long>(Constants.MaxKeysPerNode);
        sut.Count.Should().Be(0);
        sut.Delete(123);
        sut.Count.Should().Be(0);
    }

    [Property(Arbitrary = [typeof(IntArrayOfUniqueValuesArbitrary)])]
    [Trait("Category", "Property")]
    public void any_key_in_a_node_will_always_be_found_by_contains(int[] xs)
    {
        var sut = new NewLeafNode<long, long>(Constants.MaxKeysPerNode);
        foreach (var t in xs)
        {
            sut.Insert(t, t);
        }

        foreach (var x in xs)
        {
            sut.ContainsKey(x).Should().BeTrue();
        }
    }

    private bool ArrayIsInOrder(long[] a)
    {
        for (var i = 1; i < a.Length; i++)
        {
            if (a[i] < a[i - 1])
            {
                return false;
            }
        }
        return true;
    }
}

public static class IntArrayArbitrary
{
    public static Arbitrary<int[]> Values()
    {
        var size = Random.Shared.Next(2, (int)Constants.MaxKeysPerNode);
        return Arb.Generate<int>().ArrayOf(size).ToArbitrary();
    }
}

public static class IntArrayOfUniqueValuesArbitrary
{
    public static Arbitrary<int[]> Values()
    {
        var size = Random.Shared.Next(2, (int)Constants.MaxKeysPerNode);
        var result = Enumerable.Range(0, size).ToArray();
        Random.Shared.Shuffle(result);
        return Gen.Constant(result).ToArbitrary();
    }
}
