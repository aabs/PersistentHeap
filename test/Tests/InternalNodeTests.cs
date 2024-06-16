#pragma warning disable IDE1006 // Naming Styles
namespace PersistentHeap.Tests;

public class InternalNodeTests
{
    // a node created with degree N should have N-1 keys and N pointers
    [Property]
    public void InternalNode_CreatedWithDegreeN_HasNMinus1KeysAndNPointers(PositiveInt degreeVal)
    {
        var degree = 4 + degreeVal.Get;
        var node = new InternalNode<int, int>(0, degree);
        Assert.Equal(degree - 1, node.K.Length);
        Assert.Equal(degree, node.P.Length);
    }

    // a new internal node has 1 key and 2 sub-nodes
    // there should be 1 more node references than there are keys
    // adding an element to the node should increase the size by 1
    // adding an element to a full node results in an exception
    // splitting a node should result in a single node having references to two sub nodes
    // splitting a child node should push up the median key
    // inserting a duplicate key results in an exception
}

#pragma warning restore IDE1006 // Naming Styles
