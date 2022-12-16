namespace IndustrialInference.BPlusTree;

public class Node
{
    public Node()
    {
        IsLeaf = true;
    }

    public Node(long[] keys) : this()
    {
        Array.Copy(keys, K, keys.Length);
        IsLeaf = true;
        KeysInUse = keys.Count();
    }

    public Node(long[] keys, long[] pointers) : this(keys)
    {
        Array.Copy(pointers, P, pointers.Length);
        IsLeaf = false;
    }

    public bool IsEmpty => KeysInUse == 0;
    public bool IsFull => KeysInUse == Constants.MaxNodeSize;
    public bool IsLeaf { get; init; }
    public long[] K { get; set; } = new long[Constants.MaxNodeSize - 1];

    // another namer to use in case of non-leaf nodes
    public long KeysInUse { get; set; }

    public long[] P { get; set; } = new long[Constants.MaxNodeSize];

    public void Insert(long k, long r)
    {
        if (KeysInUse == 0)
        {
            K[0] = k;
            P[0] = r;
            KeysInUse++;
            return;
        }
        int insertionPoint = 0;
        while (insertionPoint < KeysInUse && K[insertionPoint] < k)
        {
            insertionPoint++;
        }

        for (long i = KeysInUse + 1; i > insertionPoint; i--)
        {
            if(i < Constants.MaxNodeSize - 1)
                K[i] = K[i - 1];
            P[i] = P[i - 1];
        }
        K[insertionPoint] = k;
        P[insertionPoint] = r;
        KeysInUse++;
    }
}
