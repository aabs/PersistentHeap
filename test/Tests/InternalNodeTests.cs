#pragma warning disable IDE1006 // Naming Styles
namespace PersistentHeap.Tests;

public class InternalNodeTests
{
    // a node created with degree N should have N-1 keys and N pointers
    [Property]
    public void internal_node_created_with_degree_n_has_n_minus_1_keys_and_n_pointers(PositiveInt degreeVal)
    {
        var degree = 4 + degreeVal.Get;

        CreateAndPopulateLeafNodes(degree, out var leafNode1, out var leafNode2);
        var node = new InternalNode<int, int>([degree], [leafNode1, leafNode2], degree);
        Assert.Equal(degree, node.K.Arr.Length);
        Assert.Equal(degree+1, node.P.Arr.Length);
    }

    [Property]
    public void a_new_internal_node_has_1_key_and_2_sub_nodes(PositiveInt degreeVal)
    {
        var degree = 4 + degreeVal.Get;
        CreateAndPopulateLeafNodes(degree, out var leafNode1, out var leafNode2);
        var node = new InternalNode<int, int>([degree], [leafNode1, leafNode2], degree);
        Assert.Equal(degree, node.K[0]);
        Assert.Same(leafNode1, node.P[0]);
        Assert.Same(leafNode2, node.P[1]);
    }

    // a new internal node has 1 key and 2 sub-nodes
    [Property]
    public void creating_a_new_node_with_empty_keys_or_pointers_results_in_exceptions(PositiveInt degreeVal)
    {
        var degree = 4 + degreeVal.Get;
        CreateAndPopulateLeafNodes(degree, out var leafNode1, out var leafNode2);
        try
        {
            _ = new InternalNode<int, int>([], [leafNode1, leafNode2], degree);
            Assert.Fail("should have thrown on empty keys");
        }
        catch (BPlusTreeException) { }
        try
        {
            _ = new InternalNode<int, int>([degree], [], degree);
            Assert.Fail("should have thrown on empty pointers");
        }
        catch (BPlusTreeException) { }
    }

    // a new internal node has 1 key and 2 sub-nodes
    [Property]
    public void the_sizes_of_keys_and_pointers_should_always_match_otherwise_exception(PositiveInt keysVal, PositiveInt nodesVal)
    {
        var degree = 10;
        var numKeys = keysVal.Get;
        var numNodes = nodesVal.Get;
        if (numKeys < 1 || numNodes < 2 || numKeys >= degree || numNodes > degree)
        {
            return;
        }
        var nodes = CreateSomeLeafNodes(degree, numNodes).ToArray();
        try
        {
            _ = new InternalNode<int, int>(Enumerable.Range(1, numKeys).ToArray(), nodes, degree);
            if (numKeys != numNodes - 1)
            {
                Assert.Fail("should have thrown on mismatched arrays");
            }
        }
        catch (BPlusTreeException) { }
    }

    [Fact]
    public void adding_an_element_to_the_node_should_increase_the_size_by_1()
    {
        var sut = CreateInternalNode(4);
        var ln = CreateSomeLeafNodes(4, 1).First();
        sut.Insert(sut.K[0]+1, ln);
        sut.Count.Should().Be(2);
        ArrayIsInOrder(sut.K.Arr);
    }

    // there should be 1 more node references than there are keys
    // adding an element to a full node results in an exception
    // splitting a node should result in a single node having references to two sub nodes
    // splitting a child node should push up the median key
    // inserting a duplicate key results in an exception

    #region Helper Functions

    private static void CreateAndPopulateLeafNodes(int degree, out NewLeafNode<int, int> leafNode1, out NewLeafNode<int, int> leafNode2)
    {
        // Create and populate two leaf nodes for insertion into the node being tested
        leafNode1 = new NewLeafNode<int, int>(degree);
        leafNode2 = new NewLeafNode<int, int>(degree);

        // Populate leafNode1 with keys and values
        for (int i = 0; i < degree - 1; i++)
        {
            leafNode1.K[i] = i;
            leafNode1.V[i] = i * 10;
        }

        // Populate leafNode2 with keys and values
        for (int i = 0; i < degree - 1; i++)
        {
            leafNode2.K[i] = i + degree - 1;
            leafNode2.V[i] = (i + degree - 1) * 10;
        }
    }
    private static IEnumerable<NewLeafNode<int, int>> CreateSomeLeafNodes(int degree, int numNodes)
    {
        var leafNode = new NewLeafNode<int, int>(degree);
        for (var x = 0; x < numNodes; x++)
        {
            for (var i = 0; i < degree-1; i++)
            {
                leafNode.K[i] = (x * degree) + i;
                leafNode.V[i] = i;
            }

        }
        yield return leafNode;
    }

    private InternalNode<int, int> CreateInternalNode(int degree)
    {
        NewLeafNode<int, int> leafNode1, leafNode2;
        CreateAndPopulateLeafNodes(degree, out leafNode1, out leafNode2);
        return new InternalNode<int, int>([degree], [leafNode1, leafNode2], degree);
    }

    private bool ArrayIsInOrder(int[] a)
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

    #endregion
}

#pragma warning restore IDE1006 // Naming Styles
