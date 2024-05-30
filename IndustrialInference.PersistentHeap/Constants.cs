namespace IndustrialInference.BPlusTree;

public static class Constants
{
#if DEBUG
    public const int MaxKeysPerNode = 4;
#else
    public const int MaxKeysPerNode = 100;
#endif
}
