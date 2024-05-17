namespace IndustrialInference.BPlusTree;

public class InternalNode<TKey, TVal> : Node<TKey, TVal>
where TKey : IComparable<TKey>
{
    public InternalNode(int id):base(id)
    {
        Keys = new TKey[Constants.MaxNodeSize - 1];
        P = new int[Constants.MaxNodeSize];
    }

    public InternalNode(int id, TKey[] keys) : this(id)
    {
        Array.Copy(keys, Keys, keys.Length);
        KeysInUse = keys.Count();
    }

    public InternalNode(int id, TKey[] keys, int[] pointers) : this(id, keys)
    {
        Array.Copy(pointers, P, pointers.Length);
    }

    public int[] P { get; set; }

    public override void Delete(TKey k)
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

        Keys[KeysInUse - 1] = default;
        P[KeysInUse - 1] = P[KeysInUse];
        P[KeysInUse] = default;
        KeysInUse--;
    }

    public override void Insert(TKey k, TVal r, bool overwriteOnEquality = true)
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
            throw new BPlusTreeException("empty internal node should have been a leaf node?");
            Keys[0] = k;
            //P[0] = r;
            KeysInUse++;
            return;
        }

        var insertionPoint = 0;
        while (insertionPoint < KeysInUse && Keys[insertionPoint].CompareTo(k) < 0)
        {
            insertionPoint++;
        }

        if (insertionPoint == Keys.Length)
        {
            OverfullNodeException.Throw("Insertion would cause overflow");
        }

        if (Keys[insertionPoint].CompareTo(k) == 0)
        {
            if (insertionPoint >= KeysInUse)
            {
                // if we get here, we have a value that is identical to the default value the node
                // keys are initialised to, but we are writing beyond the end of the area currently
                // in use, so we need to insert as usual (incrementing keys in use etc.)
                Keys[insertionPoint] = k;
                // not sure what to do here (for an internal node)
                //P[insertionPoint] = r;
                KeysInUse++;
                return;
            }

            // we have a match. Overwrite if allowed to.
            if (overwriteOnEquality)
            {
                // not sure what to do here (for an internal node)
                //P[insertionPoint] = r;
            }

            return;
        }

        for (var i = KeysInUse + 1; i > insertionPoint; i--)
        {
            if (i < Keys.Length - 1)
            {
                Keys[i] = Keys[i - 1];
            }

            P[i] = P[i - 1];
        }

        Keys[insertionPoint] = k;
        // not sure what to do here (for an internal node)
        //P[insertionPoint] = r;
        KeysInUse++;
    }
}
