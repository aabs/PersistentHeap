namespace IndustrialInference.BPlusTree;

using System.Diagnostics;
using System.Text;

[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public abstract class Node<TKey, TVal>
    where TKey : IComparable<TKey>
{
#pragma warning disable IDE1006 // Naming Styles
    protected int _keysInUse;
    protected readonly int degree;
#pragma warning restore IDE1006 // Naming Styles

    public Node(int id, int degree)
    {
        ID = id;
        this.degree = degree;
    }

    public int Count => KeysInUse;
    public int ID { get; init; }

    public bool IsEmpty => KeysInUse == 0;
    public bool IsFull => KeysInUse == degree;
    public TKey[] K { get; set; }
    public int KeysInUse { get => _keysInUse; set => _keysInUse = value; }

    public abstract void Delete(TKey k);

    public virtual void Insert(TKey k, TVal r, bool overwriteOnEquality = true)
    { }

    #region Searching

    public (TKey, TKey) KeyRange => (K[0], K[KeysInUse - 1]);
    public TKey MinKey => K[0];
    public TKey MaxKey => K[KeysInUse - 1];

    public bool ContainsKey(TKey key) => Array.BinarySearch(K, 0, KeysInUse, key) >= 0;

    protected int FindInsertionPoint<T>(T[] array, int valuesInUse, T value)
        where T : IComparable<T>
    {
        var idx = Array.BinarySearch(array, 0, valuesInUse, value);
        return idx >= 0 ? idx : ~idx;
    }

    #endregion Searching

    #region Linkage

    public int NextNode { get; set; } = -1;
    public int ParentNode { get; set; } = -1;
    public int PreviousNode { get; set; } = -1;

    #endregion Linkage

    protected virtual string DebuggerDisplay()
    {
        var sb = new StringBuilder();
        sb.AppendFormat("({0}) ", ID);
        RenderArray(sb, K, KeysInUse);
        return sb.ToString();
    }

    protected void RenderArray<T>(StringBuilder sb, T[] xs, int num)
    {
        sb.Append("[ ");
        var sep = "";

        foreach (var item in xs[..num])
        {
            sb.Append(sep);
            sb.Append(item);
            sep = " | ";
        }
        
        foreach (var item in xs[num..])
        {
            sb.Append(sep);
            sb.Append('/');
        }

        sb.Append(" ]");
    }
}
