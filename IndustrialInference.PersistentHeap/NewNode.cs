namespace IndustrialInference.BPlusTree;

using System.Diagnostics;

[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public abstract class NewNode<TKey, TVal>
    where TKey : IComparable<TKey>
{
    protected int Degree { get; init; }

    public NewNode(int degree)
    {
        BPlusTreeException.ThrowIf(degree < 4);
        Degree = degree;
        K = new(degree);
    }

    public ManagedArray<TKey> K { get; set; }

    public bool ContainsKey(TKey key) => Array.BinarySearch(K.Arr, 0, K.Count, key) >= 0;

    #region Linkage

    public NewNode<TKey, TVal>? NextNode { get; set; }
    public InternalNode<TKey, TVal>? ParentNode { get; set; }
    public NewNode<TKey, TVal>? PreviousNode { get; set; }
    public bool IsFull => K.IsFull;
    public int Count => K.Count;
    public TKey Min => K[0];
    public TKey Max => K[K.Count - 1];

    #endregion Linkage

    protected virtual string DebuggerDisplay()
        => new BPlusTreeRenderer<TKey, TVal>().Render(this, 0);
}
