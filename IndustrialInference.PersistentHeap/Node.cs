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
    public int ID { get; init; }

    public TKey[] Keys { get; set; }
    public int Count => KeysInUse;
    public bool IsEmpty => KeysInUse == 0;
    public bool IsFull => KeysInUse == Constants.MaxNodeSize;
    public int KeysInUse { get => _keysInUse; set => _keysInUse = value; }
    public abstract void Delete(TKey k);
    public virtual void Insert(TKey k, TVal r, bool overwriteOnEquality = true) { }

    #region Searching
    public bool ContainsKey(TKey key) => FindElementIndexByBinarySearch(Keys, (int)KeysInUse, key) != -1;
    protected int FindInsertionPointByBinarySearch<T>(T[] array, int valuesInUse, T value)
    where T : IComparable<T>
    {
        int low = 0;
        int high = valuesInUse-1;
        while (low <= high)
        {
            int mid = (low + high) / 2;
            if (array[mid].CompareTo(value) == 0)
            {
                return mid;
            }
            if (array[mid].CompareTo(value) < 0)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }
        return low;
    }
    protected int FindElementIndexByBinarySearch<T>(T[] array, int valuesInUse, T value)
        where T : IComparable<T>
    {
        int low = 0;
        int high = valuesInUse - 1;
        while (low <= high)
        {
            int mid = (low + high) / 2;
            if (array[mid].CompareTo(value) == 0)
            {
                return mid;
            }
            if (array[mid].CompareTo(value) < 0)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }
        return -1;
    }


    #endregion
    #region Linkage
    public int ParentNode { get; set; } = -1;
    public int PreviousNode { get; set; } = -1;
    public int NextNode { get; set; } = -1;
    #endregion

}
