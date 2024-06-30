namespace IndustrialInference.BPlusTree;

using System.Diagnostics;

[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public class InternalNode<TKey, TVal> : NewNode<TKey, TVal>
    where TKey : IComparable<TKey>
{
    public ManagedArray<NewNode<TKey, TVal>> P { get; set; }
    public int Count
    {
        get
        {
            return K.Count;
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, Degree);
            K.Count = value;
            K.Count = value+1;
        }
    }

    public InternalNode(TKey[] keys, NewNode<TKey, TVal>[] nodes, int degree) : base(degree)
    {
        // validations
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(keys);
        BPlusTreeException.ThrowIf(degree < 4, "Degree must be at least 4");
        BPlusTreeException.ThrowIf(keys.Length > degree, "Keys array is too large");
        BPlusTreeException.ThrowIf(nodes.Length > degree+1, "Nodes array is too large");
        BPlusTreeException.ThrowIf(keys.Length != nodes.Length - 1,
            "Keys and nodes arrays must be one point different in size");

        P = new(degree+1);
        K.OverwriteWith(keys, keys.Length);
        P.OverwriteWith(nodes, nodes.Length);
        foreach (var n in nodes)
        {
            n.ParentNode = this;
        }
    }

    public InternalNode(NewNode<TKey, TVal> nlo, NewNode<TKey, TVal> nhi, TKey key, int degree)
        : this([key], [nlo, nhi], degree)
    {
    }

    public void Delete(TKey k) => throw new NotImplementedException();

    public void Insert(TKey k, NewNode<TKey, TVal> n, bool overwriteOnEquality = true)
    {
        // validations
        ArgumentNullException.ThrowIfNull(k);
        ArgumentNullException.ThrowIfNull(n);
        BPlusTreeException.ThrowIf(K.Arr.Any(x => x.CompareTo(k) == 0),
            "You cannot insert a duplicate key to an internal node");
        OverfullNodeException.ThrowIf(K.IsFull, "Node is full", this);

        var idx = K.FindInsertionPoint(k); // this will be the point of insertion into both K and P
        K.InsertAt(k, idx);
        P.InsertAt(n, idx+1);
    }

    // k is the key that has been pulled-up (in the case of an internal node) or copied up (in the
    // case of a splitting leaf node) during the splitting of a child node.
    public void Insert(TKey k, NewNode<TKey, TVal> n) => Insert(k, n, true);

    public (InternalNode<TKey, TVal>, InternalNode<TKey, TVal>) Split()
    {
        var divisionPoint = Count / 2;
        var resultLo = new InternalNode<TKey, TVal>(K.Arr[..divisionPoint], P.Arr[..divisionPoint], Degree);
        var resultHi = new InternalNode<TKey, TVal>(K.Arr[divisionPoint..K.Count], P.Arr[divisionPoint..K.Count], Degree);

        // link them together
        resultLo.NextNode = resultHi;
        resultLo.PreviousNode = PreviousNode;
        resultLo.ParentNode = ParentNode;
        resultHi.PreviousNode = resultLo;
        resultHi.NextNode = NextNode;
        resultHi.ParentNode = ParentNode;
        if (PreviousNode is not null)
        {
            PreviousNode!.NextNode = resultLo;
        }
        if (NextNode is not null)
        {
            NextNode!.PreviousNode = resultHi;
        }
        return (resultLo, resultHi);
    }

}

