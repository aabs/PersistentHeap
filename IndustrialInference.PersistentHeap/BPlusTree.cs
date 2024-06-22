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
    private readonly int degree;
    private readonly TKey? defaultKey;
    private readonly TVal? defaultValue;
    #region Public interface

    /// <summary>
    ///   Initializes a new instance of the <see cref="BPlusTree{TKey, TVal}" /> class.
    /// </summary>
    public BPlusTree(int degree = Constants.MaxKeysPerNode, TKey? defaultKey = default, TVal? defaultValue = default)
    {
        Nodes = [new LeafNode<TKey, TVal>(0, degree)];
        this.degree = degree;
        this.defaultKey = defaultKey;
        this.defaultValue = defaultValue;
    }

    public IEnumerable<InternalNodeOld<TKey, TVal>> InternalNodes
            => Nodes.Where(n => n is InternalNodeOld<TKey, TVal>).Cast<InternalNodeOld<TKey, TVal>>();

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
            => Insert(key, value, RootIndexNode);

    public void InsertIntoInternalNode(TKey key, InternalNodeOld<TKey, TVal> nodeToInsertInto, Node<TKey, TVal> nlo, Node<TKey, TVal> nhi)
    {
        try
        {
            nodeToInsertInto.Insert(key, nlo.ID, nhi.ID);
        }
        catch (OverfullNodeException)
        {
            SplitInternalNode(nodeToInsertInto, key, nlo.ID, nhi.ID);
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
        return AllLeafNodes().SelectMany(x => x.K[..x.KeysInUse]);
    }
    public IEnumerable<TVal> AllItems()
    {
        return AllLeafNodes().SelectMany(x => x.V[..x.KeysInUse]);
    }
    #endregion Public interface

    #region Node Manipulation

    private InternalNodeOld<TKey, TVal> CreateNewInternalNode(TKey key, int leftChild, int rightChild)
    {
        var newIndex = Nodes.Count;
        var n = new InternalNodeOld<TKey, TVal>(newIndex, degree, [key], [leftChild, rightChild]);
#if WIPE_UNUSED
        Array.Fill(n.K, defaultKey ?? default, 1, n.K.Length - 1);
        Array.Fill(n.P, -1, 2, n.P.Length - 2);
#endif

        Nodes.Add(n);

        return n;
    }

    private InternalNodeOld<TKey, TVal> CreateNewInternalNode(TKey[] keys, int[] children)
    {
        var newIndex = Nodes.Count;
        var n = new InternalNodeOld<TKey, TVal>(newIndex, degree, keys, children);
        Nodes.Add(n);
        return n;
    }

    private LeafNode<TKey, TVal> CreateNewLeafNode(TKey[] keys, TVal[] items)
    {
        var newIndex = Nodes.Count;
        var n = new LeafNode<TKey, TVal>(newIndex, degree, keys, items);
        Nodes.Add(n);

        return n;
    }

    private InternalNodeOld<TKey, TVal> SplitInternalNode(InternalNodeOld<TKey, TVal> nlo, TKey newKey, int loId, int hiId)
    {
        OnNodeSplitting(nlo);
        var parent = (InternalNodeOld<TKey, TVal>?)Get(nlo.ParentNode);
        // 1. create the new internal node
        // 2. transfer rhs of lo node into it
        var midPoint = nlo.K.Length / 2;
        var nhi = CreateNewInternalNode(nlo.K[midPoint..], nlo.P[midPoint..]);
        nhi.KeysInUse = nlo.KeysInUse - midPoint;
        nlo.KeysInUse = midPoint;
        nhi.ParentNode = nlo.ParentNode;

#if WIPE_UNUSED
        Array.Fill(nlo.K[midPoint..], defaultKey ?? default);
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
            parent = CreateNewInternalNode(nlo.MaxKey, nlo.ID, nhi.ID);
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
            InsertIntoInternalNode(nlo.MaxKey, parent, nlo, nhi);
        }
        OnNodeSplit(nlo, nhi);
        return parent;
    }

    private InternalNodeOld<TKey, TVal> SplitLeafNode(LeafNode<TKey, TVal> n, TKey newKey, TVal newItem)
    {
        OnNodeSplitting(n);
        var parent = (InternalNodeOld<TKey, TVal>?)Get(n.ParentNode);
        var idlo = n.PreviousNode;
        var idhi = n.NextNode;

        var midPoint = degree / 2;
        var nhi = CreateNewLeafNode(n.K[midPoint..], n.V[midPoint..]);
        n.KeysInUse = midPoint;

#if WIPE_UNUSED
        Array.Fill<TKey>(n.K, defaultKey ?? default, midPoint, n.K.Length - midPoint);
        Array.Fill<TVal>(n.Items, defaultValue ?? default, midPoint, n.Items.Length - midPoint);
#endif

        // wire up the nodes
        nhi.NextNode = n.NextNode;
        nhi.PreviousNode = n.ID;
        n.NextNode = nhi.ID;

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
            parent = CreateNewInternalNode(n.MaxKey, n.ID, nhi.ID);
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
            InsertIntoInternalNode(n.MaxKey, parent, n, nhi);
        }

        OnNodeSplit(n, nhi);
        return parent;
    }

    #endregion Node Manipulation

    #region Search Helpers


    private LeafNode<TKey, TVal> FindNodeForKey(TKey key, Node<TKey, TVal> n)
    {
        // the node for a key, is the first node whose highest key is greater than the given key.  K[i] > key => P[i] is the node
        // Reminder - P[i] is a pointer to the leaf node containing the values less than or equal to K[i].
        // If key is greater than the last key in the node, then P[KeysInUse+1] is the node reference to use.

        if (n is LeafNode<TKey, TVal> ln)
        {
            return ln;
        }

        var i = FirstIndexGreaterThanOrEqualTo(key, n);
        /*
        while (i < n.KeysInUse && n.K[i].CompareTo(key) < 0)
        {
            i++;
        }
        */
        var n2 = GetIndirect(i, (InternalNodeOld<TKey, TVal>)n);

        return FindNodeForKey(key, n2);
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

    private Node<TKey, TVal> GetIndirect(int index, InternalNodeOld<TKey, TVal> n)
    {
        var targetNodeId = n.P[index];
        if (targetNodeId == -1)
        {
            Debugger.Break();
        }
        return Nodes[targetNodeId];
    }

    private int LastIndexSmallerThan(TKey key, Node<TKey, TVal> n)
    {
        var left = 0;
        var right = n.KeysInUse - 1;
        var result = -1;

        while (left <= right)
        {
            var mid = left + (right - left) / 2;

            if (n.K[mid].CompareTo(key) < 0)
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
        var left = 0;
        var right = n.KeysInUse - 1;

        while (left <= right)
        {
            var mid = left + ((right - left) / 2);

            if (n.K[mid].CompareTo(key) >= 0)
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
