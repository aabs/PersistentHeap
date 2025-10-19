namespace IndustrialInference.BPlusTree;

using System.Diagnostics;

[DebuggerDisplay("L{DebuggerDisplay(),nq}")]
public class NewLeafNode<TKey, TVal> : NewNode<TKey, TVal>
    where TKey : IComparable<TKey>
{
    public int Count
    {
        get => K.Count;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, K.Length);
            K.Count = value;
            V.Count = value;
        }
    }
    

    public NewLeafNode(int degree) : base(degree)
    {
        V = new(degree);
    }

    public NewLeafNode(int degree, TKey[] keys, TVal[] items) : this(degree)
    {
        BPlusTreeException.ThrowIf(keys.Length != items.Length);
        ArgumentOutOfRangeException.ThrowIfLessThan(degree, keys.Length);
        K.OverwriteWith(keys, keys.Length);
        V.OverwriteWith(items, items.Length);
    }

    public ManagedArray<TVal> V { get; set; }

    public TVal? this[TKey key]
    {
        get
        {
            var index = K.IndexOf(key);
            if (index == -1)
            {
                throw new KeyNotFoundException();
            }
            return V[index];
        }
    }

    public void Delete(TKey k)
    {
        var index = K.IndexOf(k);

        if (index == -1)
        {
            return;
        }
        K.DeleteAt(index);
        V.DeleteAt(index);
    }

    public void Insert(TKey k, TVal r, bool overwriteOnEquality = true)
    {
        var indexOfKey = K.IndexOf(k);
        var knownKey = indexOfKey >= 0;

        if (knownKey && !overwriteOnEquality)
        {
            throw new BPlusTreeException("Key already exists in node and overwrite is not allowed");
        }

        OverfullNodeException.ThrowIf(!knownKey && K.IsFull, "LeafNode is full", this);


        // if we get here, then there is space for the new value

        if (K.IsEmpty)
        {
            K.Append(k);
            V.Append(r);
            return;
        }

        if (knownKey && indexOfKey < V.Count)
        {
            // it's known, but we are allowed to overwrite. So do so and return.
            V[indexOfKey] = r;
            return;
        }

        // if we get here, then we have space for a not-previously-seen key.
        // Let's work out where to put it

        var insertionIndex = K.FindInsertionPoint(k);

        // this is the index of the first element in the collection that is
        // LARGER than the value k. Our response is to move that element and everything
        // larger than it right one place, and insert the new value k into that
        // position.

        // being a leaf node, the keys and values 'line up', so their shifting treatment is the same
        K.InsertAt(k, insertionIndex);
        V.InsertAt(r, insertionIndex);
    }

    public (NewLeafNode<TKey, TVal>, NewLeafNode<TKey, TVal>) Split()
    {
        var divisionPoint = Count / 2;
        var resultLo = new NewLeafNode<TKey, TVal>(Degree, K.Arr[..divisionPoint], V.Arr[..divisionPoint]);
        var resultHi = new NewLeafNode<TKey, TVal>(Degree, K.Arr[divisionPoint..K.Count], V.Arr[divisionPoint..K.Count]);

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
