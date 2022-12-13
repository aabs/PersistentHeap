namespace IndustrialInference.BPlusTree;

public interface IPageManager<TKey> where TKey : IComparable<TKey>
{
    PageOptions Options { get; }

    int Add(Page<TKey> page);

    Page<TKey> CreatePage(bool isExternal);

    Page<TKey> Lookup(int key);
}

public class PageManager<TKey> : IPageManager<TKey> where TKey : IComparable<TKey>

{
    public PageManager(PageOptions? options)
    {
        Options = options ?? PageOptions.Default;
        Body = new List<Page<TKey>>();
    }

    public List<Page<TKey>> Body { get; }
    public PageOptions Options { get; private set; }

    public int Add(Page<TKey> page)
    {
        Body.Add(page);
        return Body.Count - 1;
    }

    public Page<TKey> CreatePage(bool isExternal)
    {
        var p = new Page<TKey>(isExternal, -1, this, Options);
        p.Location = Add(p);
        return p;
    }

    public Page<TKey> Lookup(int key)
    {
        if (key is int i)
        {
            return Body[i];
        }
        throw new InvalidCastException("Not sure how to use type TKey");
    }
}