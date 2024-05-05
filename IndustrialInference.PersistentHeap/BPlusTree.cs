namespace IndustrialInference.BPlusTree;

public class BPlusTree
{
    public BPlusTree() => Nodes = new List<Node> { new() { IsLeaf = true } };

    public List<Node> Nodes { get; init; }
    public Node Root => Nodes[(int)RootIndexNode];
    public long RootIndexNode { get; private set; }

    public int Count()
    {
        if (Nodes.Count(n => !n.IsDeleted) == 1)
        {
            return (int)Root.KeysInUse;
        }

        return (int)Nodes.Where(n => !n.IsDeleted && n.IsLeaf).Sum(n => n.KeysInUse);
    }

    public void Insert(long key, long reference)
        => InsertTree(key, reference, RootIndexNode);

    public void SafeInsertToNode(Node n, long key, long r)
    {
        try
        {
            n.Insert(key, r);
        }
        catch (OverfullNodeException)
        {
            Split(n, key, r);
        }
    }

    public void InsertTree(long key, long r, long index)
    {
        // get the node specified by the index
        var n = Get(index);
        if (n.IsDeleted)
        {
            throw new BPlusTreeException("Attempted to insert into deleted node");
        }

        if (n.ContainsKey(key))
        {
            SafeInsertToNode(n, key, r);
            return;
        }

        // if the node is full, split it first, then insert the data to the new subtree root
        if (n.IsFull)
        {
            var newParent = Split(n, key, r);
            InsertTree(key, r, newParent);
        }

        if (n.IsLeaf)
        {
            SafeInsertToNode(n, key, r);
            return;
        }

        var keyIndex = 0;
        while (n.K[keyIndex] < key && keyIndex < n.KeysInUse)
        {
            keyIndex++;
        }

        var targetNode = Get(n.P[keyIndex]);
        try
        {
            targetNode.Insert(key, r);
        }
        catch (OverfullNodeException)
        {
            var newParent = Split(targetNode, key, r);
            InsertTree(key, r, newParent);
        }


        //for (var i = 0; i < n.KeysInUse; i++)
        //{
        //    if (key < n.K[i])
        //    {
        //        InsertTree(key, r, n.P[i]);
        //        return;
        //    }
        //}

        //InsertTree(key, r, n.P[n.KeysInUse]);
    }

    public Node Search(long key) => FindNodeForKey(key, Root);

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

    private Node FindNodeForKey(long key, Node node)
    {
        if (!node.IsLeaf)
        {
            // if this node is not a leaf, then we need to search the keys to
            // find the node that contains the data we are after
            var i = 0;
            while (i < node.KeysInUse && node.K[i] < key)
            {
                i++;
            }

            if (i >= node.KeysInUse)
            {
                return Get(node.KeysInUse);
            }

            return GetIndirect(i, node);
        }

        return node;
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

        var indexIntoOldNode = 0;
        var indexIntoNewNode = 0;

        while (indexIntoOldNode < n.KeysInUse && n.K[indexIntoOldNode] < newKey)
        {
            tmpKeys[indexIntoNewNode] = n.K[indexIntoOldNode];
            indexIntoOldNode++;
            indexIntoNewNode++;
        }

        tmpKeys[indexIntoNewNode] = newKey;
        indexIntoNewNode++;
        while (indexIntoOldNode < n.KeysInUse)
        {
            tmpKeys[indexIntoNewNode] = n.K[indexIntoOldNode];
            indexIntoOldNode++;
            indexIntoNewNode++;
        }


        //for (var i = n.K.Length-1; i > 0; i--)
        //{
        //    if (n.K[i] < newKey)
        //    {
        //        tmpKeys[i + 1] = newKey;
        //    }
        //    else
        //    {
        //        tmpKeys[i + 1] = n.K[i];
        //    }
        //}

        var midPoint = tmpKeys.Length / 2;
        var child1 = CreateNewLeafNode(tmpKeys[..midPoint]);
        var child2 = CreateNewLeafNode(tmpKeys[midPoint..]);
        var newParentIndex = CreateNewInternalNode(tmpKeys[midPoint - 1], child1, child2);
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

    public (long, long)? Delete(long key)
    {
        var n = FindNodeForKey(key, Root);

        for (var i = 0; i < n.KeysInUse; i++)
        {
            if (n.K[i] == key)
            {
                var r = n.P[i];
                n.Delete(key);
                return (key, r);
            }
        }

        return null;
    }

    public bool ContainsKey(long key)
    {
        var n = FindNodeForKey(key, Root);
        return n.ContainsKey(key);
    }
}
