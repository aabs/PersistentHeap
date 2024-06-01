namespace IndustrialInference.BPlusTree;

using System.Diagnostics;
using System.Text;

[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public class InternalNode<TKey, TVal> : Node<TKey, TVal>
where TKey : IComparable<TKey>
{
    public InternalNode(int id) : base(id)
    {
        K = new TKey[Constants.MaxKeysPerNode];
        P = new int[Constants.MaxKeysPerNode + 1];
    }

    public InternalNode(int id, TKey[] keys) : this(id)
    {
        Array.Copy(keys, K, keys.Length);
        KeysInUse = keys.Count();
    }

    public InternalNode(int id, TKey[] keys, int[] pointers) : this(id, keys)
    {
        Array.Copy(pointers, P, pointers.Length);
    }

    public int[] P { get; set; }

    public override void Delete(TKey k)
    {
        var index = Array.IndexOf(K, k);

        if (index == -1)
        {
            return;
        }

        if (index + 1 == Constants.MaxKeysPerNode)
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

        K[KeysInUse - 1] = default;
        P[KeysInUse - 1] = P[KeysInUse];
        P[KeysInUse] = default;
        KeysInUse--;
    }

    /// <summary>
    /// insert a new node into the internal node
    /// </summary>
    /// <param name="k"> the max value of the lower node</param>
    /// <param name="nHiId">the id of the higher node</param>
    /// <param name="overwriteOnEquality">should allow overwrite (makes no sense)</param>
    /// <exception cref="BPlusTreeException"></exception>
    public new void Insert(TKey k, int nHiId, bool overwriteOnEquality = true)
    {
        var indexOfKey = Array.BinarySearch(K, 0, KeysInUse, k);
        var knownKey = indexOfKey >= 0;
        if (knownKey && !overwriteOnEquality)
        {
            throw new BPlusTreeException("Key already exists in node and overwrite is not allowed");
        }
        if (!knownKey && KeysInUse + 1 == K.Length)
        {
            OverfullNodeException.Throw("InternalNode is full");
        }

        if (KeysInUse == 0)
        {
            K[0] = k;
            P[0] = nHiId;
            KeysInUse++;
            return;
        }
        if (knownKey && overwriteOnEquality)
        {
            P[indexOfKey] = nHiId;
            return;
        }

        var insertionIndex = FindInsertionPoint(K, (int)KeysInUse, k);
        if (insertionIndex + 1 == K.Length)
        {
            OverfullNodeException.Throw("Insertion would cause overflow");
        }

        Array.Copy(K, insertionIndex, K, insertionIndex + 1, K.Length - (insertionIndex + 1));
        Array.Copy(P, insertionIndex+1, P, insertionIndex + 2, P.Length - (insertionIndex + 2));
        K[insertionIndex] = k;
        P[insertionIndex+1] = nHiId;
        KeysInUse++;
    }
    protected override string DebuggerDisplay()
    {
        var sb = new StringBuilder();
        sb.AppendFormat("({0}) ", ID);
        sb.Append("K");
        RenderArray(sb, K, KeysInUse);
        sb.Append("; P");
        RenderArray(sb, P, KeysInUse+1);
        return sb.ToString();
    }
}
