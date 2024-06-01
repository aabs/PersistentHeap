namespace IndustrialInference.BPlusTree;

using System.Diagnostics;

[DebuggerDisplay("L{DebuggerDisplay(),nq}")]
public class LeafNode<TKey, TVal> : Node<TKey, TVal>
    where TKey : IComparable<TKey>
{
    public LeafNode(int id) : base(id)
    {
        K = new TKey[Constants.MaxKeysPerNode];
        Items = new TVal[Constants.MaxKeysPerNode];
    }

    public LeafNode(int id, TKey[] keys, TVal[] items) : this(id)
    {
        Array.Copy(keys, K, keys.Length);
        Array.Copy(items, Items, items.Length);
        KeysInUse = keys.Count();
    }

    public TVal[] Items { get; set; }

    public TVal? this[TKey key]
    {
        get
        {
            var index = Array.IndexOf(K[..KeysInUse], key);
            if (index == -1)
            {
                throw new KeyNotFoundException();
            }
            return Items[index];
        }
    }

    public override void Delete(TKey k)
    {
        var index = Array.IndexOf(K, k);

        if (index == -1)
        {
            return;
        }

        if (index == Items.Length - 1)
        {
            // if we are here, it means that we have found the desired key, and it is the very last
            // element of a full node, so the only work required is to erase the last elements of
            // K and P
            K[index] = default;
            Items[index] = default;
            KeysInUse--;
            return;
        }

        Array.Copy(K, index + 1, K, index, K.Length - (index+1));
        Array.Copy(Items, index + 1, Items, index, Items.Length - (index+1));

        K[KeysInUse - 1] = default;
        Items[KeysInUse - 1] = default;
        KeysInUse--;
    }

    public override void Insert(TKey k, TVal r, bool overwriteOnEquality = true)
    {
        var indexOfKey = Array.BinarySearch(K[..KeysInUse], k);
        var knownKey = indexOfKey >= 0;

        if (knownKey && !overwriteOnEquality)
        {
            throw new BPlusTreeException("Key already exists in node and overwrite is not allowed");
        }

        if (!knownKey && KeysInUse == K.Length)
        {
            OverfullNodeException.Throw("LeafNode is full");
        }

        // if we get here, then there is space for the new value

        if (KeysInUse == 0)
        {
            K[0] = k;
            Items[0] = r;
            KeysInUse++;
            return;
        }

        if (knownKey)
        {
            // it's know, but we are allowed to overwrite. So do so and return.
            Items[indexOfKey] = r;
            return;
        }

        // if we get here, then we have space for a not-previously-seen key.
        // Let's work out where to put it

        var insertionIndex = FindInsertionPoint(K, (int)KeysInUse, k);

        // this is the index of the first element in the collection that is
        // LARGER than the value k. Our response is to move that element and everything
        // larger than it right one place, and insert the new value k into that
        // position.

        // being a leaf node, the keys and values 'line up', so their shifting treatment is the same
        Array.Copy(K, insertionIndex, K, insertionIndex + 1, KeysInUse - insertionIndex);
        Array.Copy(Items, insertionIndex, Items, insertionIndex + 1, KeysInUse - insertionIndex);

        // we've made space. So lets insert the value where it belongs and inc the size accordingly
        K[insertionIndex] = k;
        Items[insertionIndex] = r;
        KeysInUse++;
    }
}
