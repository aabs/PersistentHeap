namespace IndustrialInference.BPlusTree;

using System.Diagnostics;

[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public class InternalNode<TKey, TVal> : Node<TKey, TVal>
where TKey : IComparable<TKey>
{
    public Node<TKey, TVal>[] P { get; set; }
    public InternalNode(int id, int degree) : base(id, degree)
    {
        if (degree < 4)
        {
            throw new ArgumentException("Degree must be at least 4", nameof(degree));
        }
        K = new TKey[degree - 1];
        P = new Node<TKey, TVal>[degree];
    }

    public override void Delete(TKey k) => throw new NotImplementedException();
}
