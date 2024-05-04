namespace IndustrialInference.BPlusTree;

public class Node
{
    public Node() => IsLeaf = true;

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

    public long Count => KeysInUse;
    public bool IsEmpty => KeysInUse == 0;
    public bool IsFull => KeysInUse == Constants.MaxNodeSize;
    public bool IsLeaf { get; init; }
    public long[] K { get; set; } = new long[Constants.MaxNodeSize - 1];
    public long KeysInUse { get; set; }
    public long[] P { get; set; } = new long[Constants.MaxNodeSize];
    public bool IsDeleted { get; set; }

    public void Delete(long k)
    {
        var index = Array.IndexOf(K, k);

        if (index == -1)
        {
            return;
        }

        if (index + 1 == Constants.MaxNodeSize)
        {
            // if we are here, it means that we have found the desired key, and it is the very last
            // element of a full node, so the only work required is to erase the last elements of K
            // and P
            K[index] = default;
            P[index] = P[index + 1];
            P[index + 1] = default;
            KeysInUse--;
            return;
        }

        for (var i = index + 1; i < KeysInUse; i++)
        {
            K[i - 1] = K[i];
            P[i - 1] = P[i];
        }

        K[KeysInUse] = default;
        P[KeysInUse] = P[KeysInUse + 1];
        P[KeysInUse + 1] = default;
        KeysInUse--;
    }

    public void Insert(long k, long r, bool overwriteOnEquality = true)
    {
        if (KeysInUse == Constants.MaxNodeSize - 1)
        {
            OverfullNodeException.Throw("Node is full");
        }

        if (KeysInUse == 0)
        {
            K[0] = k;
            P[0] = r;
            KeysInUse++;
            return;
        }

        var insertionPoint = 0;
        while (insertionPoint < KeysInUse && K[insertionPoint] < k)
        {
            insertionPoint++;
        }

        if (insertionPoint == K.Length)
        {
            OverfullNodeException.Throw("Insertion would cause overflow");
        }

        if (K[insertionPoint] == k)
        {
            if (insertionPoint >= KeysInUse)
            {
                // if we get here, we have a value that is identical to the default value the node
                // keys are initialised to, but we are writing beyond the end of the area currently
                // in use, so we need to insert as usual (incrementing keys in use etc.)
                K[insertionPoint] = k;
                P[insertionPoint] = r;
                KeysInUse++;
                return;
            }

            // we have a match. Overwrite if allowed to.
            if (overwriteOnEquality)
            {
                P[insertionPoint] = r;
            }

            return;
        }

        for (var i = KeysInUse + 1; i > insertionPoint; i--)
        {
            if (i < Constants.MaxNodeSize - 1)
            {
                K[i] = K[i - 1];
            }

            P[i] = P[i - 1];
        }

        K[insertionPoint] = k;
        P[insertionPoint] = r;
        KeysInUse++;
    }
}
