namespace IndustrialInference.BPlusTree;

public abstract class Node<TKey, TVal>
    where TKey : IComparable<TKey>
{
    public Node(int id)
    {
        ID = id;
    }
    public int ID { get; init; }

    public TKey[] Keys { get; set; }
    public long Count => KeysInUse;
    public bool IsEmpty => KeysInUse == 0;
    public bool IsFull => KeysInUse == Constants.MaxNodeSize;
    public long KeysInUse { get; set; }
    public bool IsDeleted { get; set; }
    public bool ContainsKey(TKey key) => Keys[..(int)KeysInUse].Contains(key);
    public abstract void Delete(TKey k);
    public abstract void Insert(TKey k, TVal r, bool overwriteOnEquality = true);
}
