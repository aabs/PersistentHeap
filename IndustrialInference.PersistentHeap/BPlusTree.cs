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
            // Internal node with k keys has k+1 child pointers
            return [intlNode, .. intlNode.P.Arr[..(intlNode.Count + 1)].SelectMany(x => GetChildNodes(x))];
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
            if (Root is null)
            {
                throw new KeyNotFoundException($"Key {key} not found: tree is empty");
            }

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
        if (Root is null)
        {
            return false;
        }

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
        if (Root is null)
        {
            throw new KeyNotFoundException($"Key {key} not found: tree is empty");
        }

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
                // Split the full internal node and then retry insertion into the resulting parent
                var parentAfterSplit = SplitInternalNode(internalNode);
                InsertIntoInternalNode(key, parentAfterSplit, nodeToInsert);
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
        var childIndex = (idx < n.Count && key.CompareTo(n.K[idx]) == 0)
            ? idx + 1 // equality goes to the right partition
            : idx;
        Insert(key, value, n.P[childIndex]);
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
                // Split and insert the pending key into the appropriate new leaf; no retry needed
                SplitLeafNode(ln, key, value);
                return;
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
    public NewNode<TKey, TVal> Search(TKey key) => Root is null
        ? throw new KeyNotFoundException($"Key {key} not found: tree is empty")
        : FindNodeForKey(key, Root);

    public IEnumerable<TKey> AllKeys()
        => AllLeafNodes().SelectMany(ln => ln.K.Arr[..ln.Count]);

    public IEnumerable<TKey> AllKeys(NewNode<TKey, TVal> n)
    {
        if (n is NewLeafNode<TKey, TVal>)
        {
            return n.K.Arr[..n.Count];
        }

        if (n is InternalNode<TKey, TVal> internalNode)
            return AllLeafNodes().SelectMany(ln => ln.K.Arr[..ln.Count]);

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
        CreateNewInternalNode([key], [leftChild, rightChild]);

    private InternalNode<TKey, TVal> CreateNewInternalNode(TKey[] keys, NewNode<TKey, TVal>[] children)
    {
        var node = new InternalNode<TKey, TVal>(keys, children, degree);
        Nodes.Add(node);
        return node;
    }

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
        // For internal node split, the separator (median) is MOVED UP to the parent
        var divisionPoint = nodeToSplit.Count / 2;
        var separator = nodeToSplit.K[divisionPoint];
        var (nlo, nhi) = nodeToSplit.Split();
        // Add the new nodes to the tree
        Nodes.Add(nlo);
        Nodes.Add(nhi);
        var parent = nodeToSplit.ParentNode;
        var keyForParent = separator;

        if (parent is null)
        {
            // if the parent was null, then the leaf node was also the root, and we need to create a
            // new parent to be the new root
            parent = CreateNewInternalNode(keyForParent, nlo, nhi);
            Nodes.Remove(nodeToSplit); // Remove the old root
            Root = parent;
            return parent;
        }

        // if parent is not null, perform move-up at the correct pointer index
        // if parent is not null, perform move-up: left child stays at same pointer index
        parent.P.ReplaceValue(nodeToSplit, nlo);
        Nodes.Remove(nodeToSplit);

        // If parent is full, split it now; after split, nlo's ParentNode will be updated
        var targetParent = parent;
        if (targetParent.K.IsFull)
        {
            targetParent = SplitInternalNode(targetParent);
        }

        // Ensure we insert adjacent to the left split child
        if (!ReferenceEquals(targetParent, nlo.ParentNode))
        {
            targetParent = nlo.ParentNode;
        }
        var childIdx = targetParent.P.IndexOf(nlo);
        targetParent.K.InsertAt(keyForParent, childIdx);
        targetParent.P.InsertAt(nhi, childIdx + 1);
        nhi.ParentNode = targetParent;
        OnNodeSplit(nlo, nhi);
        return targetParent;
    }

    private void SplitLeafNode(NewLeafNode<TKey, TVal> n, TKey newKey, TVal newItem)
    {
        OnNodeSplitting(n);

        var (nlo, nhi) = n.Split();
        // Add the new nodes to the tree
        Nodes.Add(nlo);
        Nodes.Add(nhi);
        var parent = n.ParentNode;

        if (parent is null)
        {
            // if the parent was null, then the leaf node was also the root, and we need to create a
            // new parent to be the new root
            parent = CreateNewInternalNode(nhi.Min, nlo, nhi);
            // For leaf splits, don't delete the separator from nhi
            Nodes.Remove(n); // Remove the old root
            Root = parent;
            // Insert the new key into the appropriate child
            if (newKey.CompareTo(nhi.Min) >= 0)
            {
                nhi.Insert(newKey, newItem);
            }
            else
            {
                nlo.Insert(newKey, newItem);
            }
            return;
        }

        // if parent is not null, then we need to perform the copy-up operation
        parent!.P.ReplaceValue(n, nlo);
        Nodes.Remove(n);
        InsertIntoInternalNode(nhi.Min, parent, nhi);
        // For leaf splits, don't delete the separator from nhi
        // Insert the new key into the appropriate child
        if (newKey.CompareTo(nhi.Min) >= 0)
        {
            nhi.Insert(newKey, newItem);
        }
        else
        {
            nlo.Insert(newKey, newItem);
        }
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
        // Keys are sorted. For each separator K[i]:
        // - if key < K[i], go to P[i]
        // - if key == K[i], go to P[i+1] (right partition)
        // Otherwise continue scanning.
        for (var i = 0; i < internalNode.Count; i++)
        {
            var cmp = key.CompareTo(internalNode.K[i]);
            if (cmp < 0)
            {
                return internalNode.P[i];
            }
            if (cmp == 0)
            {
                return internalNode.P[i + 1];
            }
        }
        // If key is greater than all keys, use the rightmost child
        return internalNode.P[internalNode.Count];
    }

    private IEnumerable<NewLeafNode<TKey, TVal>> AllLeafNodes()
    {
        // Find the true leftmost leaf by descending from the root
        if (Root is null)
        {
            yield break;
        }

        NewNode<TKey, TVal> cursor = Root;
        while (cursor is InternalNode<TKey, TVal> i)
        {
            cursor = i.P[0];
        }

        var leaf = cursor as NewLeafNode<TKey, TVal>;
        while (leaf != null)
        {
            yield return leaf;
            leaf = leaf.NextNode as NewLeafNode<TKey, TVal>;
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
