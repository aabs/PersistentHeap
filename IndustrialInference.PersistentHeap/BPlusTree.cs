namespace IndustrialInference.BPlusTree;

/// <summary>
///   Represents a B+ tree data structure.
/// </summary>
/// <typeparam name="TKey">
///   The type of the keys in the tree.
/// </typeparam>
/// <typeparam name="TVal">
///   The type of the values in the tree.
/// </typeparam>
/// <remarks>
///   <para>Here is a quick representation of the data structure for aiding visualisation
///     <code>
///<![CDATA[
///
///[P,13,P,/,P]
///|    |
////      \_______
////               \
///[P,5,P,10,P]               [P,20,P,/,P]
////    |     \_               |     \
////     |       \              |      \
///[1,4] -> [5,9] -> [10,12] -> [13,18] -> [20,/]
///]]>
///     </code>
///   </para>
/// </remarks>
public class BPlusTree<TKey, TVal>
    where TKey : IComparable<TKey>
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="BPlusTree{TKey, TVal}" /> class.
    /// </summary>
    public BPlusTree() => Nodes = [new LeafNode<TKey, TVal>(0)];

    public IEnumerable<InternalNode<TKey, TVal>> InternalNodes
            => Nodes.Where(n => n is InternalNode<TKey, TVal>).Cast<InternalNode<TKey, TVal>>();

    public IEnumerable<LeafNode<TKey, TVal>> LeafNodes
            => Nodes.Where(n => n is LeafNode<TKey, TVal>).Cast<LeafNode<TKey, TVal>>();

    /// <summary>
    ///   Gets the list of nodes in the tree.
    /// </summary>
    public List<Node<TKey, TVal>> Nodes { get; init; }

    /// <summary>
    ///   Gets the root node of the tree.
    /// </summary>
    public Node<TKey, TVal> Root => Nodes[RootIndexNode];

    /// <summary>
    ///   Gets or sets the index of the root node.
    /// </summary>
    public int RootIndexNode { get; private set; }

    /// <summary>
    ///   Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">
    ///   The key to search for.
    /// </param>
    /// <returns>
    ///   The value associated with the key, or null if the key is not found.
    /// </returns>
    public TVal? this[TKey key]
    {
        get
        {
            var n = FindNodeForKey(key, Root);

            return n[key];
        }
    }

    /// <summary>
    ///   Determines whether the tree contains the specified key.
    /// </summary>
    /// <param name="key">
    ///   The key to search for.
    /// </param>
    /// <returns>
    ///   true if the key is found; otherwise, false.
    /// </returns>
    public bool ContainsKey(TKey key)
    {
        var n = FindNodeForKey(key, Root);

        return n.ContainsKey(key);
    }

    /// <summary>
    ///   Gets the number of key-value pairs in the tree.
    /// </summary>
    /// <returns>
    ///   The number of key-value pairs in the tree.
    /// </returns>
    public int Count()
    {
        return LeafNodes.Sum(n => (int)n.KeysInUse);
    }

    /// <summary>
    ///   Deletes the key-value pair with the specified key from the tree.
    /// </summary>
    /// <param name="key">
    ///   The key of the key-value pair to delete.
    /// </param>
    /// <returns>
    ///   The deleted key-value pair.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    ///   Thrown when the specified key is not found in the tree.
    /// </exception>
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

    /// <summary>
    ///   Inserts a key-value pair into the tree.
    /// </summary>
    /// <param name="key">
    ///   The key of the key-value pair to insert.
    /// </param>
    /// <param name="reference">
    ///   The value of the key-value pair to insert.
    /// </param>
    public void Insert(TKey key, TVal reference)
            => InsertTree(key, reference, RootIndexNode);

    public void InsertIntoInternalNode(TKey key, InternalNode<TKey, TVal> nodeToInsertInto, Node<TKey, TVal> nodeToInsert)
    {
        try
        {
            nodeToInsertInto.Insert(key, nodeToInsert.ID);
        }
        catch (OverfullNodeException)
        {
            SplitInternalNode(nodeToInsertInto, key, nodeToInsert.ID);
        }
    }

    /// <summary>
    ///   Inserts a key-value pair into the tree starting from the specified index.
    /// </summary>
    /// <param name="key">
    ///   The key of the key-value pair to insert.
    /// </param>
    /// <param name="r">
    ///   The value of the key-value pair to insert.
    /// </param>
    /// <param name="index">
    ///   The index of the node to start the insertion from.
    /// </param>
    public void InsertTree(TKey key, TVal r, int index)
    {
        var n = FindNodeForKey(key, Get(index));
        try
        {
            n.Insert(key, r);
        }
        catch (OverfullNodeException)
        {
            SplitLeafNode(n, key, r);
        }
    }

    /// <summary>
    ///   Searches for the node containing the specified key.
    /// </summary>
    /// <param name="key">
    ///   The key to search for.
    /// </param>
    /// <returns>
    ///   The node containing the key.
    /// </returns>
    public Node<TKey, TVal> Search(TKey key) => FindNodeForKey(key, Root);

    private InternalNode<TKey, TVal> CreateNewInternalNode(TKey key, int leftChild, int rightChild)
    {
        var newIndex = Nodes.Count;
        var n = new InternalNode<TKey, TVal>(newIndex, [key], [leftChild, rightChild]);
        Nodes.Add(n);

        return n;
    }

    private InternalNode<TKey, TVal> CreateNewInternalNode(TKey[] keys, int[] children)
    {
        var newIndex = Nodes.Count;
        var n = new InternalNode<TKey, TVal>(newIndex, keys, children);
        Nodes.Add(n);
        return n;
    }

    private LeafNode<TKey, TVal> CreateNewLeafNode(TKey[] keys, TVal[] items)
    {
        var newIndex = Nodes.Count;
        var n = new LeafNode<TKey, TVal>(newIndex, keys, items);
        Nodes.Add(n);

        return n;
    }

    private LeafNode<TKey, TVal> FindNodeForKey(TKey key, Node<TKey, TVal> n)
    {
        if (n is LeafNode<TKey, TVal> ln)
        {
            return ln;
        }

        var i = 0;
        while (i < n.KeysInUse && n.Keys[i].CompareTo(key) < 0)
        {
            i++;
        }

        var n2 = GetIndirect(i, (InternalNode<TKey, TVal>)n);

        return FindNodeForKey(key, n2);
    }

    private Node<TKey, TVal>? Get(int id)
    {
        if (id < 0 || id > Nodes.Count)
        {
            return null;
        }
        return Nodes[id];
    }

    private Node<TKey, TVal> GetIndirect(int id, InternalNode<TKey, TVal> n) => Nodes[(int)n.P[id]];

    private InternalNode<TKey, TVal> SplitInternalNode(InternalNode<TKey, TVal> nlo, TKey newKey, int newRef)
    {
        OnNodeSplitting(nlo);
        var parent = (InternalNode<TKey, TVal>?)Get(nlo.ParentNode);
        // 1. create the new internal node
        // 2. transfer rhs of lo node into it
        var midPoint = nlo.Keys.Length / 2;
        var nhi = CreateNewInternalNode(nlo.Keys[midPoint..], nlo.P[midPoint..]);
        nlo.KeysInUse = midPoint;
        nhi.ParentNode = nlo.ParentNode;

        // 3. update parent ID of all transferred children
        foreach (var n in nhi.P[..(int)nhi.KeysInUse].Select(nodeId => Get(nodeId)))
        {
            if (n is not null)
            {
                n.ParentNode = nhi.ID;
            }
        }
        // 4. insert hi node into parent
        if (parent is null)
        {
            parent = CreateNewInternalNode(nlo.Keys[midPoint - 1], nlo.ID, nhi.ID);
            nlo.ParentNode = parent.ID;
            nhi.ParentNode = parent.ID;
            parent.ParentNode = -1;

            // by definition, as a node with no parent, nlo must have been the root
            RootIndexNode = parent.ID;
        }
        else
        {
            // if the parent is not null, then we insert the new child into the parent, and let it
            // work out the rest
            InsertIntoInternalNode(nlo.Keys[nlo.KeysInUse], parent, nhi);
        }
        OnNodeSplit(nlo, nhi);
        return parent;
    }

    private InternalNode<TKey, TVal> SplitLeafNode(LeafNode<TKey, TVal> n, TKey newKey, TVal newItem)
    {
        OnNodeSplitting(n);
        var parent = (InternalNode<TKey, TVal>?)Get(n.ParentNode);
        var idlo = n.PreviousNode;
        var idhi = n.NextNode;

        var midPoint = n.Keys.Length / 2;
        var nhi = CreateNewLeafNode(n.Keys[midPoint..], n.Items[midPoint..]);
        n.KeysInUse = midPoint;

        // wire up the nodes
        nhi.NextNode = n.NextNode;
        nhi.PreviousNode = n.ID;
        n.NextNode = nhi.ID;

        // now insert key into one of the leaf nodes
        if (newKey.CompareTo(nhi.Keys[0]) >= 0) // if newKey > all keys in low side node
        {
            nhi.Insert(newKey, newItem);
        }
        else
        {
            n.Insert(newKey, newItem);
        }

        // we don't create the parent node here. It gets created if the parent is full it's the
        // responsibility of the parent's insert function to wire up parentage ids

        // only if there is no parent should we create a parent here and wire it up
        if (parent is null)
        {
            parent = CreateNewInternalNode(nhi.Keys[0], n.ID, nhi.ID);
            n.ParentNode = parent.ID;
            nhi.ParentNode = parent.ID;
            parent.ParentNode = -1;

            // by definition, as a leaf with no parent, this node must be the root
            RootIndexNode = parent.ID;
        }
        else
        {
            // if the parent is not null, then we insert the new child into the parent, and let it
            // work out the rest
            nhi.ParentNode = parent.ID;
            InsertIntoInternalNode(nhi.Keys[0], parent, nhi);
        }

        OnNodeSplit(n, nhi);
        return parent;
    }

    #region Events

    public event Action<Node<TKey, TVal>,Node<TKey, TVal>> NodeSplit;

    public event Action<Node<TKey, TVal>> NodeSplitting;

    protected virtual void OnNodeSplit(Node<TKey, TVal> nlo, Node<TKey, TVal> nhi)
        => NodeSplit?.Invoke(nlo, nhi);

    protected virtual void OnNodeSplitting(Node<TKey, TVal> node)
            => NodeSplitting?.Invoke(node);

    #endregion Events
}
