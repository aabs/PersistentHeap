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

    public string Render(Node<TKey, TVal> n, int indent)
    {
        if (n is LeafNode<TKey, TVal> leafNode)
        {
            return RenderLeaf(leafNode, indent);
        }
        else if (n is InternalNodeOld<TKey, TVal> internalNode)
        {
            return RenderInternal(internalNode, indent);
        }
        else
        {
            throw new ArgumentException("Invalid node type");
        }
    }
    public string RenderLeaf(LeafNode<TKey, TVal> n, int indent)
    {
        var sb = new StringBuilder();
        sb.Append(new string(' ', 2*indent));
        sb.AppendFormat("({0}) ", n.ID);
        sb.AppendLine(Render(n.K, n.KeysInUse));
        return sb.ToString();
    }

    public string RenderInternal(InternalNodeOld<TKey, TVal> n, int indent)
    {
        var sb = new StringBuilder();
        sb.Append(new string(' ', 2*indent));
        sb.AppendFormat("({0}) ", n.ID);
        sb.Append("K");
        sb.Append(Render(n.K, n.KeysInUse));
        sb.Append("; P");
        sb.AppendLine(Render(n.P, n.KeysInUse+1));

        for (int i = 0; i < n.KeysInUse + 1; i++)
        {
            var childNode = t.Nodes[n.P[i]];
            var childIndent = indent + 1;
            var renderedChild = Render(childNode, childIndent);
            sb.Append(renderedChild);
        }

        return sb.ToString();
    }
    public string Render<T>(T[] xs, int elementsUsed)
    {
        StringBuilder sb = new();
        sb.Append("[ ");
        var sep = "";

        foreach (var item in xs[..elementsUsed])
        {
            sb.Append(sep);
            sb.Append(item);
            sep = " | ";
        }

        foreach (var item in xs[elementsUsed..])
        {
            sb.Append(sep);
            sb.Append('/');
        }

        sb.Append(" ]");
        return sb.ToString();
    }
}
