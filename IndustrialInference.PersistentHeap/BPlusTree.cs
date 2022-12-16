namespace IndustrialInference.BPlusTree;

public class BPlusTree
{
    public BPlusTree()
    {
        Nodes = new List<Node> { new Node { IsLeaf = true } };
    }

    public List<Node> Nodes { get; init; }
    public Node Root => Nodes[(int)RootIndexNode];
    public long RootIndexNode { get; init; }

    public void Insert(long key, long reference)
        => InsertTree(key, reference, RootIndexNode);

    public void InsertTree(long k, long r, long idx)
    {
        var n = Get(idx);
        if (n.IsFull)
        {
            var newParent = Split(n, k);
            InsertTree(k, r, newParent);
        }
        if (n.IsLeaf)
        {
            n.Insert(k, r);
            return;
        }

        for (int i = 0; i < n.KeysInUse; i++)
        {
            if (k < n.K[i])
            {
                InsertTree(k, r, n.P[i]);
                return;
            }
        }
        InsertTree(k, r, n.P[n.KeysInUse]);
    }

    public Node Search(long key) => SearchTree(key, Root);

    private long CreateNewInternalNode(long key, long leftChild, long rightChild)
    {
        var n = new Node(new long[] { key }, new long[] { leftChild, rightChild });
        int newIndex = Nodes.Count;
        Nodes.Add(n);
        return newIndex;
    }

    private long CreateNewLeafNode(long[] keys)
    {
        var n = new Node(keys);
        int newIndex = Nodes.Count;
        Nodes.Add(n);
        return newIndex;
    }

    private Node Get(long id) => Nodes[(int)id];

    private Node GetIndirect(long id, Node n) => Nodes[(int)n.P[id]];

    private Node SearchTree(long key, Node node)
    {
        for (int i = 0; i < node.KeysInUse; i++)
        {
            if (key < node.K[i])
            {
                return SearchTree(key, GetIndirect(i, node));
            }
        }
        return Get(node.KeysInUse);
    }

    private long Split(Node n, long newKey)
    => n.IsLeaf switch
    {
        true => SplitLeafNode(n, newKey, newKey),
        false => SplitInternalNode(n, newKey)
    };

    private long SplitInternalNode(Node n, long newKey) => throw new NotImplementedException();

    private long SplitLeafNode(Node n, long newKey, long newRef)
    {
        var x = new long[n.K.Length + 1];
        var y = new long[n.K.Length + 1];
        Array.Copy(n.K, x, n.K.Length);
        Array.Copy(n.P, y, n.P.Length);
        for (int i = n.K.Length; i > 0; i--)
        {
            if (n.K[i] < newKey)
            {
                x[i + 1] = newKey;
                y[i + 1] = newRef;
            }
            else
            {
                x[i + 1] = n.K[i];
                y[i + 1] = n.P[i];
            }
        }

        int midPoint = x.Length / 2;
        long midVal = x[x.Length / 2];
        var child1 = CreateNewLeafNode(n.K[0..midPoint]);
        var child2 = CreateNewLeafNode(n.K[midPoint..]);
        var newParent = CreateNewInternalNode(newKey, child1, child2);
        return newParent;
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
