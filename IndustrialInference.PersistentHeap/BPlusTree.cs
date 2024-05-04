namespace IndustrialInference.BPlusTree;

public class BPlusTree
{
    public BPlusTree() => Nodes = new List<Node> { new() { IsLeaf = true } };

    public List<Node> Nodes { get; init; }
    public Node Root => Nodes[(int)RootIndexNode];
    public long RootIndexNode { get; private set; }

    public int Count() => (int)Nodes.Where(n => !n.IsDeleted && n.IsLeaf).Sum(n => n.KeysInUse);

    public void Insert(long key, long reference)
        => InsertTree(key, reference, RootIndexNode);

    public void InsertTree(long key, long r, long index)
    {
        // get the node specified by the index
        var n = Get(index);

        // if the node is full, split it first, then insert the data to the new subtree root
        if (n.IsFull)
        {
            var newParent = Split(n, key, r);
            InsertTree(key, r, newParent);
        }

        if (n.IsLeaf)
        {
            try
            {
                n.Insert(key, r);
            }
            catch (OverfullNodeException )
            {
                var newParent = Split(n, key, r);
                InsertTree(key, r, newParent);
            }
            return;
        }

        for (var i = 0; i < n.KeysInUse; i++)
        {
            if (key < n.K[i])
            {
                InsertTree(key, r, n.P[i]);
                return;
            }
        }

        InsertTree(key, r, n.P[n.KeysInUse]);
    }

    public Node Search(long key) => SearchTree(key, Root);

    private long CreateNewInternalNode(long key, long leftChild, long rightChild)
    {
        var n = new Node(new[] { key }, new[] { leftChild, rightChild });
        var newIndex = Nodes.Count;
        Nodes.Add(n);
        return newIndex;
    }

    private long CreateNewLeafNode(long[] keys)
    {
        var n = new Node(keys);
        var newIndex = Nodes.Count;
        Nodes.Add(n);
        return newIndex;
    }

    private Node Get(long id) => Nodes[(int)id];

    private Node GetIndirect(long id, Node n) => Nodes[(int)n.P[id]];

    private Node SearchTree(long key, Node node)
    {
        for (var i = 0; i < node.KeysInUse; i++)
        {
            if (key < node.K[i])
            {
                return SearchTree(key, GetIndirect(i, node));
            }
        }

        return Get(node.KeysInUse);
    }

    private long Split(Node n, long newKey, long newRef)
        => n.IsLeaf switch
        {
            true => SplitLeafNode(n, newKey, newRef),
            false => SplitInternalNode(n, newKey, newRef)
        };

    private long SplitInternalNode(Node n, long newKey, long newRef) => throw new NotImplementedException();

    private long SplitLeafNode(Node n, long newKey, long newRef)
    {
        // make arrays 1 bigger than the overflowing node, to hold the sorted data
        var tmpKeys = new long[n.K.Length + 1];

        // copy contents of node across to the new arrays, inserting the new key
        for (var i = n.K.Length-1; i > 0; i--)
        {
            if (n.K[i] < newKey)
            {
                tmpKeys[i + 1] = newKey;
            }
            else
            {
                tmpKeys[i + 1] = n.K[i];
            }
        }

        var midPoint = tmpKeys.Length / 2;
        var child1 = CreateNewLeafNode(tmpKeys[..midPoint]);
        var child2 = CreateNewLeafNode(tmpKeys[midPoint..]);
        var newParentIndex = CreateNewInternalNode(midPoint, child1, child2);
        var oldNode = DeleteNode(n);
        if (Root == oldNode)
        {
            RootIndexNode = newParentIndex;
        }
        return newParentIndex;
    }

    private Node DeleteNode(Node oldNode)
    {
        //Nodes.Remove(oldNode);
        oldNode.IsDeleted = true;
        return oldNode;
    }

    /*
     *                [P,13,P,/,P]
     *                 |    |
     *                /      \_______
     *               /               \
     *     [P,5,P,10,P]               [P,20,P,/,P]
     *     /    |     \_               |     \
     *    /     |       \              |      \
     * [1,4] -> [5,9] -> [10,12] -> [13,18] -> [20,/]
     *
     */

}
