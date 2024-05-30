namespace IndustrialInference.BPlusTree;

public abstract class Node<TKey, TVal>
    where TKey : IComparable<TKey>
{
#pragma warning disable IDE1006 // Naming Styles
    protected int _keysInUse;
#pragma warning restore IDE1006 // Naming Styles

    public Node(int id)
    {
        ID = id;
    }

    public int Count => KeysInUse;
    public int ID { get; init; }

    public bool IsEmpty => KeysInUse == 0;
    public bool IsFull => KeysInUse == Constants.MaxKeysPerNode;
    public TKey[] Keys { get; set; }
    public int KeysInUse { get => _keysInUse; set => _keysInUse = value; }

    public abstract void Delete(TKey k);

    public virtual void Insert(TKey k, TVal r, bool overwriteOnEquality = true)
    { }

    #region Searching

    public (TKey, TKey) KeyRange => (Keys[0], Keys[KeysInUse - 1]);

    public bool ContainsKey(TKey key) => Array.BinarySearch(Keys[..KeysInUse], key) >= 0;

    protected int FindInsertionPoint<T>(T[] array, int valuesInUse, T value)
        where T : IComparable<T>
    {
        var idx = Array.BinarySearch(array[..valuesInUse], value);
        return idx >= 0 ? idx : ~idx;
    }

    #endregion Searching

    #region Linkage

    public int NextNode { get; set; } = -1;
    public int ParentNode { get; set; } = -1;
    public int PreviousNode { get; set; } = -1;

    #endregion Linkage
}
