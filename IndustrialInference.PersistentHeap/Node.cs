namespace IndustrialInference.BPlusTree;

public abstract class Node
{
    public long[] Keys { get; set; }
    public long Count => KeysInUse;
    public bool IsEmpty => KeysInUse == 0;
    public bool IsFull => KeysInUse == Constants.MaxNodeSize;
    public long KeysInUse { get; set; }
    public bool IsDeleted { get; set; }
    public bool ContainsKey(long key) => Keys[..(int)KeysInUse].Contains(key);
    public abstract void Delete(long k);
    public abstract void Insert(long k, long r, bool overwriteOnEquality = true);
}
