namespace IndustrialInference.BPlusTree;

public class InternalNode : Node
{
    public InternalNode()
    {
        Keys = new long[Constants.MaxNodeSize - 1];
        P = new long[Constants.MaxNodeSize];
    }

    public InternalNode(long[] keys) : this()
    {
        Array.Copy(keys, Keys, keys.Length);
        KeysInUse = keys.Count();
    }

    public InternalNode(long[] keys, long[] pointers) : this(keys)
    {
        Array.Copy(pointers, P, pointers.Length);
    }

    public long[] P { get; set; }

    public override void Delete(long k)
    {
        var index = Array.IndexOf(Keys, k);

        if (index == -1)
        {
            return;
        }

        if (index + 1 == Constants.MaxNodeSize)
        {
            // if we are here, it means that we have found the desired key, and it is the very last
            // element of a full node, so the only work required is to erase the last elements of Keys
            // and P
            Keys[index] = default;
            P[index] = P[index + 1];
            P[index + 1] = default;
            KeysInUse--;
            return;
        }

        for (var i = index + 1; i < KeysInUse; i++)
        {
            Keys[i - 1] = Keys[i];
            P[i - 1] = P[i];
        }

        Keys[KeysInUse-1] = default;
        P[KeysInUse-1] = P[KeysInUse];
        P[KeysInUse] = default;
        KeysInUse--;
    }

    public override void Insert(long k, long r, bool overwriteOnEquality = true)
    {
        var knownKey = ContainsKey(k);
        if (knownKey && !overwriteOnEquality)
        {
            throw new BPlusTreeException("Key already exists in node and overwrite is not allowed");
        }
        if (!knownKey && KeysInUse == Constants.MaxNodeSize - 1)
        {
            OverfullNodeException.Throw("InternalNode is full");
        }

        if (KeysInUse == 0)
        {
            Keys[0] = k;
            P[0] = r;
            KeysInUse++;
            return;
        }

        var insertionPoint = 0;
        while (insertionPoint < KeysInUse && Keys[insertionPoint] < k)
        {
            insertionPoint++;
        }

        if (insertionPoint == Keys.Length)
        {
            OverfullNodeException.Throw("Insertion would cause overflow");
        }

        if (Keys[insertionPoint] == k)
        {
            if (insertionPoint >= KeysInUse)
            {
                // if we get here, we have a value that is identical to the default value the node
                // keys are initialised to, but we are writing beyond the end of the area currently
                // in use, so we need to insert as usual (incrementing keys in use etc.)
                Keys[insertionPoint] = k;
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
                Keys[i] = Keys[i - 1];
            }

            P[i] = P[i - 1];
        }

        Keys[insertionPoint] = k;
        P[insertionPoint] = r;
        KeysInUse++;
    }
}
