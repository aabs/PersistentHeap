namespace IndustrialInference.BPlusTree;

public class BPlusTree<TKey, TVal>
    where TKey : IComparable<TKey>
{
    public BPlusTree() => Nodes = new List<Node<TKey, TVal>> { new LeafNode<TKey, TVal>() };

    public List<Node<TKey, TVal>> Nodes { get; init; }
    public Node<TKey, TVal> Root => Nodes[RootIndexNode];
    public int RootIndexNode { get; private set; }

    public int Count()
    {
        if (Nodes.Count(n => !n.IsDeleted) == 1)
        {
            return (int)Root.KeysInUse;
        }

        return (int)Nodes.Where(n => !n.IsDeleted && n is LeafNode<TKey, TVal>).Sum(n => n.KeysInUse);
    }

    public void Insert(TKey key, TVal reference)
        => InsertTree(key, reference, RootIndexNode);

    public void SafeInsertToNode(Node<TKey, TVal> n, TKey key, TVal r)
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

    public void InsertTree(TKey key, TVal r, int index)
    {
        // get the node specified by the index
        var n = FindNodeForKey(key, Get(index));
        if (n.IsDeleted)
        {
            throw new BPlusTreeException("Attempted to insert into deleted node");
        }

        // if the node is full, split it first, then insert the data to the new subtree root
        SafeInsertToNode(n, key, r);
        /*
        if (n.ContainsKey(key))
        {
            SafeInsertToNode(n, key, r);
            return;
        }

        if (n.IsFull)
        {
            var newParent = Split(n, key, r);
            InsertTree(key, r, newParent);
        }

        if (n is LeafNode)
        {
            SafeInsertToNode(n, key, r);
            return;
        }

        // if we get here, we are dealing with an internal node
        var intN = n as InternalNode;

        var keyIndex = 0;
        while (intN.Keys[keyIndex] < key && keyIndex < intN.KeysInUse)
        {
            keyIndex++;
        }

        var targetNode = Get(intN.P[keyIndex]);
        try
        {
            targetNode.Insert(key, r);
        }
        catch (OverfullNodeException)
        {
            var newParent = Split(targetNode, key, r);
            InsertTree(key, r, newParent);
        }
        */
    }

    public Node<TKey, TVal> Search(TKey key) => FindNodeForKey(key, Root);

    private int CreateNewInternalNode(TKey key, int leftChild, int rightChild)
    {
        var n = new InternalNode<TKey, TVal>(new[] { key }, new[] { leftChild, rightChild });
        var newIndex = Nodes.Count;
        Nodes.Add(n);
        return newIndex;
    }

    private int CreateNewLeafNode(TKey[] keys, TVal[] items)
    {
        var n = new LeafNode<TKey, TVal>(keys, items);
        var newIndex = Nodes.Count;
        Nodes.Add(n);
        return newIndex;
    }

    private Node<TKey, TVal> Get(int id) => Nodes[(int)id];

    private Node<TKey, TVal> GetIndirect(int id, InternalNode<TKey, TVal> n) => Nodes[(int)n.P[id]];

    private LeafNode<TKey, TVal> FindNodeForKey(TKey key, Node<TKey, TVal> n)
    {
        if (n is LeafNode<TKey, TVal> ln)
            return ln;

        // if this n is not a leaf, then we need to search the keys to
        // find the n that contains the data we are after
        var i = 0;
        while (i < n.KeysInUse && n.Keys[i].CompareTo(key) < 0)
        {
            i++;
        }

        var n2 = GetIndirect(i, (InternalNode<TKey, TVal>)n);
        return FindNodeForKey(key, n2);
    }

    private long Split(Node<TKey, TVal> n, TKey newKey, TVal newRef)
    {
        if (n is LeafNode<TKey, TVal> ln)
        {
            return SplitLeafNode(ln, newKey, newRef);
        }

        if (n is InternalNode<TKey, TVal> internalNode)
        {
            return SplitInternalNode(internalNode, newKey, newRef);
        }

        throw new BPlusTreeException("Unrecognised node type");
    }

    private long SplitInternalNode(InternalNode<TKey, TVal> n, TKey newKey, TVal newRef) => throw new NotImplementedException();

    private long SplitLeafNode(LeafNode<TKey, TVal> n, TKey newKey, TVal newItem)
    {
        // make arrays 1 bigger than the overflowing node, to hold the sorted data
        var tmpKeys = new TKey[n.KeysInUse + 1];
        var tmpItems = new TVal[n.KeysInUse + 1];

        // copy contents of node across to the new arrays, inserting the new key

        var indexIntoOldNode = 0;
        var indexIntoNewNode = 0;

        while (indexIntoOldNode < n.KeysInUse && n.Keys[indexIntoOldNode].CompareTo(newKey) < 0)
        {
            tmpKeys[indexIntoNewNode] = n.Keys[indexIntoOldNode];
            tmpItems[indexIntoNewNode] = n.Items[indexIntoOldNode];
            indexIntoOldNode++;
            indexIntoNewNode++;
        }

        tmpKeys[indexIntoNewNode] = newKey;
        tmpItems[indexIntoNewNode] = newItem;
        indexIntoNewNode++;
        while (indexIntoOldNode < n.KeysInUse)
        {
            tmpKeys[indexIntoNewNode] = n.Keys[indexIntoOldNode];
            tmpItems[indexIntoNewNode] = n.Items[indexIntoOldNode];
            indexIntoOldNode++;
            indexIntoNewNode++;
        }


        //for (var i = n.Keys.Length-1; i > 0; i--)
        //{
        //    if (n.Keys[i] < newKey)
        //    {
        //        tmpKeys[i + 1] = newKey;
        //    }
        //    else
        //    {
        //        tmpKeys[i + 1] = n.Keys[i];
        //    }
        //}

        var midPoint = tmpKeys.Length / 2;
        var child1 = CreateNewLeafNode(tmpKeys[..midPoint], tmpItems[..midPoint]);
        var child2 = CreateNewLeafNode(tmpKeys[midPoint..], tmpItems[midPoint..]);
        var newParentIndex = CreateNewInternalNode(tmpKeys[midPoint - 1], child1, child2);
        var oldNode = DeleteNode(n);
        if (Root == oldNode)
        {
            RootIndexNode = newParentIndex;
        }

        return newParentIndex;
    }

    private Node<TKey, TVal> DeleteNode(Node<TKey, TVal> oldInternalNode)
    {
        //Nodes.Remove(oldInternalNode);
        oldInternalNode.IsDeleted = true;
        return oldInternalNode;
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

    public (TKey, TVal)? Delete(TKey key)
    {
        var n = FindNodeForKey(key, Root);

        for (var i = 0; i < n.KeysInUse; i++)
        {
            if (n.Keys[i].CompareTo(key) == 0)
            {
                var r = n.Items[i];
                n.Delete(key);
                return (key, r);
            }
        }

        throw new KeyNotFoundException();
    }

    public bool ContainsKey(TKey key)
    {
        var n = FindNodeForKey(key, Root);
        return n.ContainsKey(key);
    }

    public TVal? this[TKey key]
    {
        get
        {
            var n = FindNodeForKey(key, Root);

            return n[key];
        }
    }


}
