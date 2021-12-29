namespace IndustrialInference.BPlusTree;

public record struct KeyPtr<TKey> where TKey : IComparable<TKey>

{
    public TKey Key;
    public int Ptr;

    public KeyPtr(TKey key, int ptr) : this()
    {
        Key = key;
        Ptr = ptr;
    }

    public override string? ToString()
    {
        if (int.MinValue is TKey sentinel)
        {
            var k = Comparer<TKey>.Default.Compare(Key, sentinel) == 0 ? "*" : Key.ToString();
            var p = Ptr == int.MinValue ? "/" : Ptr.ToString();
            return $"{k}→{p}";
        }
        return string.Empty;
    }
}