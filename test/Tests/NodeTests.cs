namespace PersistentHeap.Tests;

using DotNext;
using Random = System.Random;

public class LeafNodePropertyTests
{
    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    public void adding_a_key_to_a_node_that_already_contains_it_does_not_add_anything(int[] xs)
    {
        var sut = new LeafNode<long, long>(0);
        foreach (var i in xs)
        {
            sut.Insert(i, 123);
        }

        // now let's try to insert one of the already inserted keys, and see what happens
        var expected = sut.Count;
        sut.Insert(xs[0], 123);
        var actual = sut.Count;
        expected.Should().Be(actual);
    }

    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    public void adding_a_new_key_to_a_non_full_node_increases_keys_by_one(int[] xs)
    {
        if (xs.Length > Constants.MaxKeysPerNode)
        {
            return;
        }
        var sut = new LeafNode<long, long>(0);
        foreach (var i in xs)
        {
            sut.Insert(i, 123, overwriteOnEquality: true);
        }
        
        sut.Count.Should().Be(xs.Distinct().Count());
    }

    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    public void adding_keys_to_a_node_leaves_the_node_keys_in_order(int[] xs)
    {
        var sut = new LeafNode<long, long>(0);
        foreach (var i in xs)
        {
            sut.Insert(i, 123, overwriteOnEquality: true);
        }

        var expected = xs.Distinct().OrderBy(x => x).Select(i => (long)i).ToArray();
        var actual = sut.Keys[..(int)sut.Count];
        expected.Should().BeEquivalentTo(actual);
    }

    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    public void removing_a_key_from_a_node_reduces_the_number_of_keys_by_one(int[] xs)
    {
        var sut = new LeafNode<long, long>(0);
        foreach (var i in xs)
        {
            sut.Insert(i, 123, overwriteOnEquality: true);
        }

        var expected = sut.Count -1;
        sut.Delete(xs[0]);
        var actual = sut.Count;
        expected.Should().Be(actual);
    }

    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    public void removing_a_key_from_a_node_leaves_the_node_keys_in_order(int[] xs)
    {
        var sut = new LeafNode<long, long>(0);
        foreach (var i in xs)
        {
            sut.Insert(i, 123, overwriteOnEquality: true);
        }

        if (xs.Length == 0 || sut.Count == 0)
        {
            return;
        }
        sut.Delete(xs[0]);

        var a = sut.Keys[..(int)sut.Count];
        var b = a.OrderBy(x => x);
        for (int i = 0; i < (int)sut.Count; i++)
        {
            a[i].Should().Be(b.ElementAt(i));
        }
    }

    [Fact]
    public void removing_an_element_from_an_empty_node_changes_nothing()
    {
        var sut = new LeafNode<long, long>(0);
        sut.Count.Should().Be(0);
        sut.Delete(123);
        sut.Count.Should().Be(0);
    }

    [Property(Arbitrary = [typeof(IntArrayOfUniqueValuesArbitrary)])]
    public void any_key_in_a_node_will_always_be_found_by_contains(int[] xs)
    {
        var sut = new LeafNode<long, long>(0);
        for (int i = 0; i < xs.Length; i++)
        {
            sut.Insert(xs[i], xs[i]);
        }

        foreach (var x in xs)
        {
            sut.ContainsKey(x).Should().BeTrue();
        }
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
