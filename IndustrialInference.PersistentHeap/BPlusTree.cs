namespace IndustrialInference.BPlusTree;


/// <summary>
/// Represents a B+ tree data structure.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the tree.</typeparam>
/// <typeparam name="TVal">The type of the values in the tree.</typeparam>
/// <remarks>
/// <para> Here is a quick representation of the data structure for aiding visualisation
/// <code>
/// <![CDATA[
/// 
///                [P,13,P,/,P]
///                 |    |
///                /      \_______
///               /               \
///     [P,5,P,10,P]               [P,20,P,/,P]
///     /    |     \_               |     \
///    /     |       \              |      \
/// [1,4] -> [5,9] -> [10,12] -> [13,18] -> [20,/]
/// ]]>
/// </code>
/// </para>
/// </remarks>
public class BPlusTree<TKey, TVal>
    where TKey : IComparable<TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BPlusTree{TKey, TVal}"/> class.
    /// </summary>
    public BPlusTree() => Nodes = [new LeafNode<TKey, TVal>(0)];

    /// <summary>
    /// Gets the list of nodes in the tree.
    /// </summary>
    public List<Node<TKey, TVal>> Nodes { get; init; }

    /// <summary>
    /// Gets the root node of the tree.
    /// </summary>
    public Node<TKey, TVal> Root => Nodes[RootIndexNode];

    public IEnumerable<LeafNode<TKey, TVal>> LeafNodes
        => Nodes.Where(n => n is LeafNode<TKey, TVal>).Cast<LeafNode<TKey, TVal>>();

    public IEnumerable<InternalNode<TKey, TVal>> InternalNodes
        => Nodes.Where(n => n is InternalNode<TKey, TVal>).Cast<InternalNode<TKey, TVal>>();

    /// <summary>
    /// Gets or sets the index of the root node.
    /// </summary>
    public int RootIndexNode { get; private set; }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to search for.</param>
    /// <returns>The value associated with the key, or null if the key is not found.</returns>
    public TVal? this[TKey key]
    {
        get
        {
            var n = FindNodeForKey(key, Root);

            return n[key];
        }
    }

    /// <summary>
    /// Determines whether the tree contains the specified key.
    /// </summary>
    /// <param name="key">The key to search for.</param>
    /// <returns>true if the key is found; otherwise, false.</returns>
    public bool ContainsKey(TKey key)
    {
        var n = FindNodeForKey(key, Root);

        return n.ContainsKey(key);
    }

    /// <summary>
    /// Gets the number of key-value pairs in the tree.
    /// </summary>
    /// <returns>The number of key-value pairs in the tree.</returns>
    public int Count()
    {
        return LeafNodes.Sum(n => (int)n.KeysInUse);
    }

    /// <summary>
    /// Deletes the key-value pair with the specified key from the tree.
    /// </summary>
    /// <param name="key">The key of the key-value pair to delete.</param>
    /// <returns>The deleted key-value pair.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified key is not found in the tree.</exception>
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
    /// Inserts a key-value pair into the tree.
    /// </summary>
    /// <param name="key">The key of the key-value pair to insert.</param>
    /// <param name="reference">The value of the key-value pair to insert.</param>
    public void Insert(TKey key, TVal reference)
            => InsertTree(key, reference, RootIndexNode);

    /// <summary>
    /// Inserts a key-value pair into the tree starting from the specified index.
    /// </summary>
    /// <param name="key">The key of the key-value pair to insert.</param>
    /// <param name="r">The value of the key-value pair to insert.</param>
    /// <param name="index">The index of the node to start the insertion from.</param>
    public void InsertTree(TKey key, TVal r, int index)
    {
        var n = FindNodeForKey(key, Get(index));
        SafeInsertToNode(n, key, r);
    }

    /// <summary>
    /// Inserts a key-value pair into the specified node, handling node splitting if necessary.
    /// </summary>
    /// <param name="n">The node to insert the key-value pair into.</param>
    /// <param name="key">The key of the key-value pair to insert.</param>
    /// <param name="r">The value of the key-value pair to insert.</param>
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

    /// <summary>
    /// Searches for the node containing the specified key.
    /// </summary>
    /// <param name="key">The key to search for.</param>
    /// <returns>The node containing the key.</returns>
    public Node<TKey, TVal> Search(TKey key) => FindNodeForKey(key, Root);

    private int CreateNewInternalNode(TKey key, int leftChild, int rightChild)
    {
        var newIndex = Nodes.Count;
        var n = new InternalNode<TKey, TVal>(newIndex, [key], [leftChild, rightChild]);
        Nodes.Add(n);

        return newIndex;
    }

    private int CreateNewLeafNode(TKey[] keys, TVal[] items)
    {
        var newIndex = Nodes.Count;
        var n = new LeafNode<TKey, TVal>(newIndex, keys, items);
        Nodes.Add(n);

        return newIndex;
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

    private Node<TKey, TVal> Get(int id) => Nodes[(int)id];

    private Node<TKey, TVal> GetIndirect(int id, InternalNode<TKey, TVal> n) => Nodes[(int)n.P[id]];

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

    private long SplitInternalNode(InternalNode<TKey, TVal> n, TKey newKey, TVal newRef) =>
        throw new NotImplementedException();

    private long SplitLeafNode(LeafNode<TKey, TVal> n, TKey newKey, TVal newItem)
    {
        var midPoint = n.Keys.Length / 2;
        var c1 = CreateNewLeafNode(n.Keys[midPoint..(int)n.KeysInUse], n.Items[midPoint..(int)n.KeysInUse]);
        n.KeysInUse = midPoint;
        var newParentIndex = CreateNewInternalNode(n.Keys[midPoint - 1], n.ID, c1);
        if (Root == n)
        {
            RootIndexNode = newParentIndex;
        }
        var np = Get(newParentIndex) as InternalNode<TKey, TVal>;
        Insert(newKey, newItem);
        //np.Insert(newKey, newItem);
        return newParentIndex;
    }
}
