namespace IndustrialInference.BPlusTree;

using System.Diagnostics;
using System.Text;

[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public class InternalNodeOld<TKey, TVal> : OldNode<TKey, TVal>
where TKey : IComparable<TKey>
{
    public InternalNodeOld(int id, int degree) : base(id, degree)
    {
        K = new TKey[degree - 1];
        P = new int[degree];
    }

    public InternalNodeOld(int id, int degree, TKey[] keys) : this(id, degree)
    {
        Array.Copy(keys, K, keys.Length);
        KeysInUse = keys.Count();
    }

    public InternalNodeOld(int id, int degree, TKey[] keys, int[] pointers) : this(id, degree, keys)
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

        if (index + 1 == degree)
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
    public new void Insert(TKey k, int pLo, int pHi, bool overwriteOnEquality = true)
    {
        // Case: Insertion at front of internal node, given two subnode IDs (pLo, pHi) and the new key value k.
        //       Insertion point is 0, new key to add is 7, new pHi is 4, pLo is still 0
        //       K[10,15,/,/,/]; P[0, 1, 3,/,/,/] => K[7, 10, 15]; P[0, 4, 1, 3]
        var indexOfKey = Array.BinarySearch(K, 0, KeysInUse, k);
        var knownKey = indexOfKey >= 0;
        if (knownKey && !overwriteOnEquality)
        {
            throw new BPlusTreeException("Key already exists in node and overwrite is not allowed");
        }
        if (KeysInUse == K.Length)
        {
            OverfullNodeException.Throw("InternalNodeOld is full");
        }

        if (KeysInUse == 0)
        {
            K[0] = k;
            P[0] = pHi;
            KeysInUse++;
            return;
        }
        if (knownKey && overwriteOnEquality)
        {
            P[indexOfKey] = pHi;
            return;
        }

        var insertionIndex = FindInsertionPoint(K, (int)KeysInUse, k);
        Array.Copy(K, insertionIndex, K, insertionIndex + 1, K.Length - (insertionIndex + 1));
        Array.Copy(P, insertionIndex+1, P, insertionIndex + 2, P.Length - (insertionIndex + 2));
        K[insertionIndex] = k;
        P[insertionIndex] = pLo;
        P[insertionIndex+1] = pHi;
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
