namespace IndustrialInference.BPlusTree;
using System.Text;

public class BPlusTreeRenderer<TKey, TVal>
    where TKey : IComparable<TKey>
{
    BPlusTree<TKey, TVal> t;
    public string Render(BPlusTree<TKey, TVal> t)
    {
        this.t = t;
        return Render(t.Root, 0);
    }

    public string Render(NewNode<TKey, TVal> n, int indent)
    {
        if (n is NewLeafNode<TKey, TVal> leafNode)
        {
            return RenderLeaf(leafNode, indent);
        }
        else if (n is InternalNode<TKey, TVal> internalNode)
        {
            return RenderInternal(internalNode, indent);
        }
        else
        {
            throw new ArgumentException("Invalid node type");
        }
    }
    public string RenderLeaf(NewLeafNode<TKey, TVal> n, int indent)
    {
        var sb = new StringBuilder();
        sb.Append(new string(' ', 2*indent));
        sb.Append('L');
        sb.AppendLine(Render(n.K));
        return sb.ToString();
    }

    public string RenderInternal(InternalNode<TKey, TVal> n, int indent)
    {
        var sb = new StringBuilder();
        sb.Append(new string(' ', 2*indent));
        sb.Append("I[");
        sb.Append("K");
        sb.Append(Render(n.K));
        sb.AppendLine("]");
        foreach(var p in n.P.Arr[..(n.Count + 1)])
        {
            sb.Append(Render(p, indent + 1));
        }



        return sb.ToString();
    }
    public string Render<T>(ManagedArray<T> xs)
    {
        StringBuilder sb = new();
        sb.Append("[ ");
        var sep = "";

        foreach (var item in xs.Arr[..xs.Count])
        {
            sb.Append(sep);
            sb.Append(item);
            sep = " | ";
        }

        foreach (var item in xs.Arr[xs.Count..])
        {
            sb.Append(sep);
            sb.Append('/');
        }

        sb.Append(" ]");
        return sb.ToString();
    }
}
