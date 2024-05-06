namespace IndustrialInference.BPlusTree;

public class LeafNode<TKey, TVal> : Node<TKey, TVal>
    where TKey : IComparable<TKey>
{
    public LeafNode()
    {
        Keys = new TKey[Constants.MaxNodeSize];
        Items = new TVal[Constants.MaxNodeSize];
    }
    public LeafNode(TKey[] keys, TVal[] items) : this()
    {
        Array.Copy(keys, Keys, keys.Length);
        Array.Copy(items, Items, items.Length);
        KeysInUse = keys.Count();
    }


    public TVal[] Items { get; set; }
    public override void Delete(TKey k)
    {
        var index = Array.IndexOf(Keys, k);

        if (index == -1)
        {
            return;
        }

        if (index + 1 == Items.Length-1)
        {
            // if we are here, it means that we have found the desired key, and it is the very last
            // element of a full node, so the only work required is to erase the last elements of Keys
            // and P
            Keys[index] = default;
            Items[index] = Items[index + 1];
            Items[index + 1] = default;
            KeysInUse--;
            return;
        }

        for (var i = index + 1; i < KeysInUse; i++)
        {
            Keys[i - 1] = Keys[i];
            Items[i - 1] = Items[i];
        }

        Keys[KeysInUse-1] = default;
        Items[KeysInUse-1] = Items[KeysInUse];
        Items[KeysInUse] = default;
        KeysInUse--;
    }

    public override void Insert(TKey k, TVal r, bool overwriteOnEquality = true) 
    {
        var knownKey = ContainsKey(k);
        if (knownKey && !overwriteOnEquality)
        {
            throw new BPlusTreeException("Key already exists in node and overwrite is not allowed");
        }
        if (!knownKey && KeysInUse+1 == Items.Length)
        {
            OverfullNodeException.Throw("InternalNode is full");
        }

        if (KeysInUse == 0)
        {
            Keys[0] = k;
            Items[0] = r;
            KeysInUse++;
            return;
        }

        var insertionPoint = 0;
        while (insertionPoint < KeysInUse && Keys[insertionPoint].CompareTo(k) < 0)
        {
            insertionPoint++;
        }

        if (insertionPoint+1 == Keys.Length)
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
                Items[insertionPoint] = r;
                KeysInUse++;
                return;
            }

            // we have a match. Overwrite if allowed to.
            if (overwriteOnEquality)
            {
                Items[insertionPoint] = r;
            }

            return;
        }

        for (var i = KeysInUse + 1; i > insertionPoint; i--)
        {
            Keys[i] = Keys[i - 1];
            Items[i] = Items[i - 1];
        }

        Keys[insertionPoint] = k;
        Items[insertionPoint] = r;
        KeysInUse++;
    }
}
