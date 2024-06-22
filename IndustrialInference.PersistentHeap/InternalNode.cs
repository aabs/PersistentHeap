namespace IndustrialInference.BPlusTree;

using System.Diagnostics;
using System.Reflection;

[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public class InternalNode<TKey, TVal> : Node<TKey, TVal>
where TKey : IComparable<TKey>
{
    public Node<TKey, TVal>[] P { get; set; }
    public InternalNode(TKey[] keys, Node<TKey, TVal>[] nodes, int id, int degree) : base(id, degree)
    {
        if (degree < 4)
        {
            throw new ArgumentException("Degree must be at least 4", nameof(degree));
        }

        if (keys.Length > degree - 1)
        {
            throw new ArgumentException("Keys array is too large", nameof(keys));
        }

        if (nodes is null)
        {
            throw new ArgumentNullException(nameof(nodes));
        }

        if (keys is null)
        {
            throw new ArgumentNullException(nameof(keys));
        }

        if (nodes.Length > degree)
        {
            throw new ArgumentException("Nodes array is too large", nameof(nodes));
        }

        if (keys.Length != nodes.Length -1)
        {
            throw new BPlusTreeException("Keys and nodes arrays must be one point different in size");
        }

        K = new TKey[degree - 1];
        P = new Node<TKey, TVal>[degree];
        Array.Copy(keys, K, keys.Length);
        Array.Copy(nodes, P, nodes.Length);
    }
    public InternalNode(Node<TKey, TVal> nlo, Node<TKey, TVal> nhi, TKey key, int id, int degree)
        : this([key], [nlo, nhi], id, degree)
    {
    }

    public override void Delete(TKey k) => throw new NotImplementedException();
}
