namespace PersistentHeap.Tests;

using DotNext;
using Random = System.Random;

public class NodePropertyTests
{
    // Adding a key to a node that already contains it doesn't add anything
    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    public void adding_a_key_to_a_node_that_already_contains_it_does_not_add_anything(int[] xs)
    {
        var sut = new Node();
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

    // Adding a key to a non-empty node adds it
    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    public void adding_a_new_key_to_a_non_full_node_increases_keys_by_one(int[] xs)
    {
        if (xs.Length > Constants.MaxNodeSize)
        {
            return;
        }
        var sut = new Node();
        foreach (var i in xs)
        {
            sut.Insert(i, 123, overwriteOnEquality: true);
        }
        
        sut.Count.Should().Be(xs.Distinct().Count());
    }

    // invariant - adding keys to a node leaves the node keys in order
    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    public void adding_keys_to_a_node_leaves_the_node_keys_in_order(int[] xs)
    {
        var sut = new Node();
        foreach (var i in xs)
        {
            sut.Insert(i, 123, overwriteOnEquality: true);
        }

        var expected = xs.Distinct().OrderBy(x => x).Select(i => (long)i).ToArray();
        var actual = sut.K[..(int)sut.Count];
        expected.Should().BeEquivalentTo(actual);
    }

    // removing a key from a node reduces the number of keys by 1
    [Property(Arbitrary = [typeof(IntArrayArbitrary)])]
    public void removing_a_key_from_a_node_reduces_the_number_of_keys_by_one(int[] xs)
    {
        var sut = new Node();
        foreach (var i in xs)
        {
            sut.Insert(i, 123, overwriteOnEquality: true);
        }

        var expected = sut.Count -1;
        sut.Delete(xs[0]);
        var actual = sut.Count;
        expected.Should().Be(actual);
    }

    // removing a key from a node leaves the keys in order

}
public static class IntArrayArbitrary
{
    public static Arbitrary<int[]> Values()
    {
        var size = Random.Shared.Next(2, (int)Constants.MaxNodeSize);
        return Arb.Generate<int>().ArrayOf(size).ToArbitrary();
    }
}
