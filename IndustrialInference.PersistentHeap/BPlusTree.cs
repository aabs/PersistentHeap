namespace IndustrialInference.BPlusTree;

using System.Diagnostics;
using System.Net;

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
    private readonly TKey? defaultKey;
    private readonly TVal? defaultValue;
    #region Public interface

    /// <summary>
    ///   Initializes a new instance of the <see cref="BPlusTree{TKey, TVal}" /> class.
    /// </summary>
    public BPlusTree(TKey? defaultKey = default, TVal? defaultValue = default)
    {
        Nodes = [new LeafNode<TKey, TVal>(0)];
        this.defaultKey = defaultKey;
        this.defaultValue = defaultValue;
    }

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
    /// <param name="value">
    ///   The value of the key-value pair to insert.
    /// </param>
    public void Insert(TKey key, TVal value)
            => Insert(key, value, RootIndexNode);

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
    /// <param name="value">
    ///   The value of the key-value pair to insert.
    /// </param>
    /// <param name="index">
    ///   The index of the node to start the insertion from.
    /// </param>
    public void Insert(TKey key, TVal value, int index)
    {
        var n = FindNodeForKey(key, Get(index));
        try
        {
            n.Insert(key, value);
        }
        catch (OverfullNodeException)
        {
            SplitLeafNode(n, key, value);
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

    public IEnumerable<TKey> AllKeys()
    {
        return AllLeafNodes().SelectMany(x => x.Keys[..x.KeysInUse]);
    }
    public IEnumerable<TVal> AllItems()
    {
        return AllLeafNodes().SelectMany(x => x.Items[..x.KeysInUse]);
    }
    #endregion Public interface

    #region Node Manipulation

    private InternalNode<TKey, TVal> CreateNewInternalNode(TKey key, int leftChild, int rightChild)
    {
        var newIndex = Nodes.Count;
        var n = new InternalNode<TKey, TVal>(newIndex, [key], [leftChild, rightChild]);
#if WIPE_UNUSED
        Array.Fill(n.Keys, defaultKey ?? default, 1, n.Keys.Length - 1);
        Array.Fill(n.P, -1, 2, n.P.Length - 2);
#endif

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

#if WIPE_UNUSED
        Array.Fill(nlo.Keys[midPoint..], defaultKey ?? default);
        Array.Fill(nlo.P[midPoint..], -1);
#endif

        // 3. update parent ID of all transferred children
        foreach (var n in nhi.P[..nhi.KeysInUse].Select(nodeId => Get(nodeId)))
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

#if WIPE_UNUSED
        Array.Fill<TKey>(n.Keys, defaultKey ?? default, midPoint, n.Keys.Length - midPoint);
        Array.Fill<TVal>(n.Items, defaultValue ?? default, midPoint, n.Items.Length - midPoint);
#endif

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

    #endregion Node Manipulation

    #region Search Helpers

    private LeafNode<TKey, TVal> FindNodeForKey(TKey key, Node<TKey, TVal> n)
    {
        try
        {
            switch (n)
            {
                case LeafNode<TKey, TVal> ln:
                    return ln;
                case InternalNode<TKey, TVal> inn:
                    var i = Array.BinarySearch(inn.Keys, 0, inn.KeysInUse, key);
                    i = i >= 0 ? i : ~i;
                    i = Math.Clamp(i, 0, inn.KeysInUse-1);
                    try
                    {
                        var n2 = GetIndirect(i, inn);
                        return FindNodeForKey(key, n2);
                    }
                    catch (Exception e) when (Debugger.IsAttached)
                    {
                        _ = e;
                        Debugger.Break();
                    }
                    return LeafNodes.FirstOrDefault();
                default:
                    throw new BPlusTreeException("Unknown node type");
            }
        }
        catch (IndexOutOfRangeException e)
        {
            throw;
        }
    }

    IEnumerable<LeafNode<TKey, TVal>> AllLeafNodes()
    {
        var firstLeafNode = LeafNodes.FirstOrDefault(n => n.PreviousNode == -1);

        if (firstLeafNode != null)
        {
            var currentNode = firstLeafNode;

            while (currentNode != null)
            {
                yield return currentNode;

                if (currentNode.NextNode != -1)
                {
                    currentNode = Get(currentNode.NextNode) as LeafNode<TKey, TVal>;
                }
                else
                {
                    break;
                }
            }
        }
    }

    private Node<TKey, TVal>? Get(int id)
    {
        if (id < 0 || id > Nodes.Count)
        {
            return null;
        }
        return Nodes[id];
    }

    private Node<TKey, TVal> GetIndirect(int index, InternalNode<TKey, TVal> n)
    {
        var targetNodeId = n.P[index];
        return Nodes[targetNodeId];
    }

    private int LastIndexSmallerThan(TKey key, Node<TKey, TVal> n)
    {
        int left = 0;
        int right = n.KeysInUse - 1;
        int result = -1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;

            if (n.Keys[mid].CompareTo(key) < 0)
            {
                result = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        return result;
    }
    private int FirstIndexGreaterThanOrEqualTo(TKey key, Node<TKey, TVal> n)
    {
        int left = 0;
        int right = n.KeysInUse - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;

            if (n.Keys[mid].CompareTo(key) >= 0)
            {
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }

        return left;
    }

    #endregion Search Helpers

    #region Events

    public event Action<Node<TKey, TVal>, Node<TKey, TVal>> NodeSplit;

    public event Action<Node<TKey, TVal>> NodeSplitting;

    protected virtual void OnNodeSplit(Node<TKey, TVal> nlo, Node<TKey, TVal> nhi)
        => NodeSplit?.Invoke(nlo, nhi);

    protected virtual void OnNodeSplitting(Node<TKey, TVal> node)
            => NodeSplitting?.Invoke(node);

    #endregion Events
}
