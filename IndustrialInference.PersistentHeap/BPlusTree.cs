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
///       [P,13,P,/,P]
///        |    |
///       /      \_________________
///      /                         \
///     [P,5,P,10,P]               [P,20,P,/,P]
///     /    |     \_               |     \
///    /     |       \              |      \
///   [1,4] -> [5,9] -> [10,12] -> [13,18] -> [20,/]
///]]>
///     </code>
///   </para>
/// </remarks>
public class BPlusTree<TKey, TVal>
    where TKey : IComparable<TKey>
{
    private readonly int degree;

    #region Public interface

    /// <summary>
    ///   Initializes a new instance of the <see cref="BPlusTree{TKey, TVal}" /> class.
    /// </summary>
    public BPlusTree(int degree = Constants.MaxKeysPerNode)
    {
        Root = new NewLeafNode<TKey, TVal>(degree);
        this.degree = degree;
    }

    public IEnumerable<InternalNode<TKey, TVal>> InternalNodes
            => Nodes.Where(n => n is InternalNode<TKey, TVal>).Cast<InternalNode<TKey, TVal>>();

    public IEnumerable<NewLeafNode<TKey, TVal>> LeafNodes
            => Nodes.Where(n => n is NewLeafNode<TKey, TVal>).Cast<NewLeafNode<TKey, TVal>>();

    /// <summary>
    ///   Gets the list of nodes in the tree.
    /// </summary>
    public List<NewNode<TKey, TVal>> Nodes => GetChildNodes(Root).ToList();

    IEnumerable<NewNode<TKey, TVal>> GetChildNodes(NewNode<TKey, TVal> root)
    {
        if (root is NewLeafNode<TKey, TVal>)
        {
            return [root];
        }

        if (root is InternalNode<TKey, TVal> intlNode)
        {
            return [intlNode, .. intlNode.P.Arr[..intlNode.Count].SelectMany(x => GetChildNodes(x))];
        }

        return [];
    }

    /// <summary>
    ///   Gets the root node of the tree.
    /// </summary>
    public NewNode<TKey, TVal> Root { get; set; }


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
    public int Count() => LeafNodes.Sum(n => n.Count);

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

        for (var i = 0; i < n.Count; i++)
        {
            if (n.K[i].CompareTo(key) == 0)
            {
                var r = n.V[i];
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
    /// <param name="value">
    ///   The value of the key-value pair to insert.
    /// </param>
    public void Insert(TKey key, TVal value)
            => Insert(key, value, Root);

    public void InsertIntoInternalNode(TKey key, InternalNode<TKey, TVal> nodeToInsertInto, NewNode<TKey, TVal> nodeToInsert)
    {
        try
        {
            nodeToInsertInto.Insert(key, nodeToInsert);
        }
        catch (OverfullNodeException e)
        {
            if (e.SourceNode is InternalNode<TKey, TVal> internalNode)
            {
                SplitInternalNode(internalNode);
            }
        }
    }

    /// <summary>
    ///   Inserts a key-value pair into the tree starting from the specified index.
    /// </summary>
    /// <param name="n"></param>
    /// <param name="key">
    ///     The key of the key-value pair to insert.
    /// </param>
    /// <param name="value">
    ///     The value of the key-value pair to insert.
    /// </param>
    /// <param name="index">
    ///   The index of the node to start the insertion from.
    /// </param>
    public void InsertIntoLeaf(NewLeafNode<TKey, TVal> n, TKey key, TVal value)
    {
        n.Insert(key, value);
    }

    public void InsertIntoInternal(InternalNode<TKey, TVal> n, TKey key, TVal value)
    {
        var idx = n.K.FindInsertionPoint(key);
        Insert(key, value, n.P[idx]);
    }

    public void Insert(TKey key, TVal value, NewNode<TKey, TVal> n)
    {
        try
        {
            if (n is NewLeafNode<TKey, TVal> ln)
            {
                InsertIntoLeaf(ln, key, value);
            }
            else if (n is InternalNode<TKey, TVal> internalNode)
            {
                InsertIntoInternal(internalNode, key, value);
            }
        }
        catch (OverfullNodeException e)
        {

            if (e.SourceNode is NewLeafNode<TKey, TVal> ln)
            {
                SplitLeafNode(ln, key, value);
            }
            else if (e.SourceNode is InternalNode<TKey, TVal> internalNode)
            {
                SplitInternalNode(internalNode);
            }

            // try again, now that there should be some room
            Insert(key, value);
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
    public NewNode<TKey, TVal> Search(TKey key) => FindNodeForKey(key, Root);

    public IEnumerable<TKey> AllKeys() => AllKeys(Root);

    public IEnumerable<TKey> AllKeys(NewNode<TKey, TVal> n)
    {
        if (n is NewLeafNode<TKey, TVal>)
        {
            return n.K.Arr[..n.Count];
        }

        if (n is InternalNode<TKey, TVal> internalNode)
        {
            return internalNode.P.Arr.SelectMany(AllKeys);
        }

        return [];
    }
    public IEnumerable<TVal> AllItems()
    {
        return AllLeafNodes().SelectMany(x => x.V.Arr[..x.Count]);
    }
    #endregion Public interface

    #region Node Manipulation

    private InternalNode<TKey, TVal> CreateNewInternalNode(TKey key, NewNode<TKey, TVal> leftChild,
        NewNode<TKey, TVal> rightChild) =>
        new([key], [leftChild, rightChild], degree);

    private InternalNode<TKey, TVal> CreateNewInternalNode(TKey[] keys, NewNode<TKey, TVal>[] children) =>
        new(keys, children, degree);

    private NewLeafNode<TKey, TVal> CreateNewLeafNode(TKey[] keys, TVal[] items)
    {
        var newIndex = Nodes.Count;
        var n = new NewLeafNode<TKey, TVal>(degree, keys, items);
        Nodes.Add(n);

        return n;
    }

    private InternalNode<TKey, TVal> SplitInternalNode(InternalNode<TKey, TVal> nodeToSplit)
    {
        OnNodeSplitting(nodeToSplit);
        var (nlo, nhi) = nodeToSplit.Split();
        var parent = nodeToSplit.ParentNode;
        var keyForParent = nhi.Min;

        if (parent is null)
        {
            // if the parent was null, then the leaf node was also the root, and we need to create a
            // new parent to be the new root
            parent = CreateNewInternalNode(keyForParent, nlo, nhi);
            nhi.Delete(keyForParent);
            Root = parent;
            return parent;
        }

        // if parent is not null, then we need to perform the copy-up operation
        parent!.P.ReplaceValue(nodeToSplit, nlo);
        InsertIntoInternalNode(keyForParent, parent, nhi);
        OnNodeSplit(nlo, nhi);
        return parent;
    }

    private void SplitLeafNode(NewLeafNode<TKey, TVal> n, TKey newKey, TVal newItem)
    {
        OnNodeSplitting(n);

        var (nlo, nhi) = n.Split();
        var parent = n.ParentNode;

        if (parent is null)
        {
            // if the parent was null, then the leaf node was also the root, and we need to create a
            // new parent to be the new root
            parent = CreateNewInternalNode(nlo.Max, nlo, nhi);
            Root = parent;
            return;
        }

        // if parent is not null, then we need to perform the copy-up operation
        parent!.P.ReplaceValue(n, nlo);
        InsertIntoInternalNode(nlo.Max, parent, nhi);
        /*
        var parent = n.ParentNode;
        var midPoint = degree / 2;
        var nhi = CreateNewLeafNode(n.K.Arr[midPoint..], n.V.Arr[midPoint..]);
        n.Count = midPoint;

        // wire up the nodes
        nhi.NextNode = n.NextNode;
        nhi.PreviousNode = n;
        n.NextNode = nhi;

        // now insert key into one of the leaf nodes
        if (newKey.CompareTo(nhi.K[0]) >= 0) // if newKey > all keys in low side node
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
            parent = CreateNewInternalNode(n.K[n.Count-1], n, nhi);
            n.ParentNode = parent;
            nhi.ParentNode = parent;
            parent.ParentNode = null;

            // by definition, as a leaf with no parent, this node must be the root
            Root = parent;
        }
        else
        {
            // if the parent is not null, then we insert the new child into the parent, and let it
            // work out the rest
            nhi.ParentNode = parent;
            var p = n.ParentNode as InternalNode<TKey, TVal>;
            p.Insert(n.K[n.Count], nhi);
        }

        OnNodeSplit(n, nhi);
        return parent as InternalNode<TKey, TVal>;
        */
    }

    #endregion Node Manipulation

    #region Search Helpers


    private NewLeafNode<TKey, TVal> FindNodeForKey(TKey key, NewNode<TKey, TVal> n)
    {
        // the node for a key, is the first node whose highest key is greater than the given key.  K[i] > key => P[i] is the node
        // Reminder - P[i] is a pointer to the leaf node containing the values less than or equal to K[i].
        // If key is greater than the last key in the node, then P[KeysInUse+1] is the node reference to use.

        return n switch
        {
            NewLeafNode<TKey, TVal> ln => ln,
            InternalNode<TKey, TVal> internalNode => FindNodeForKey(key, GetChildNodeForKey(key, internalNode)),
            _ => throw new InvalidOperationException($"Unknown node type: {n.GetType()}")
        };
    }

    private NewNode<TKey, TVal> GetChildNodeForKey(TKey key, InternalNode<TKey, TVal> internalNode)
    {
        // Find the appropriate child node based on the key
        // Keys are sorted, so find the first key that is greater than or equal to the search key
        for (int i = 0; i < internalNode.Count; i++)
        {
            if (key.CompareTo(internalNode.K[i]) <= 0)
            {
                return internalNode.P[i];
            }
        }
        // If key is greater than all keys, use the rightmost child
        return internalNode.P[internalNode.Count];
    }

    IEnumerable<NewLeafNode<TKey, TVal>> AllLeafNodes()
    {
        var firstLeafNode = LeafNodes.FirstOrDefault(n => n.PreviousNode == null);

        if (firstLeafNode != null)
        {
            var currentNode = firstLeafNode;

            while (currentNode != null)
            {
                yield return currentNode;

                if (currentNode.NextNode != null)
                {
                    currentNode = currentNode.NextNode as NewLeafNode<TKey, TVal>;
                }
                else
                {
                    break;
                }
            }
        }
    }


    #endregion Search Helpers

    #region Events

    public event Action<NewNode<TKey, TVal>, NewNode<TKey, TVal>> NodeSplit;

    public event Action<NewNode<TKey, TVal>> NodeSplitting;

    protected virtual void OnNodeSplit(NewNode<TKey, TVal> nlo, NewNode<TKey, TVal> nhi)
        => NodeSplit?.Invoke(nlo, nhi);

    protected virtual void OnNodeSplitting(NewNode<TKey, TVal> node)
            => NodeSplitting?.Invoke(node);

    #endregion Events
}
