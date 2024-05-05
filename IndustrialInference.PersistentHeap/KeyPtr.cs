namespace IndustrialInference.BPlusTree;

public record struct KeyPtr<TKey>(TKey Key, int Ptr) where TKey : IComparable<TKey>;
