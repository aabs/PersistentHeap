namespace IndustrialInference.BPlusTree;

public class BTreeSet<TKey> where TKey : IComparable<TKey>
{
    private readonly IPageManager<TKey> pageManager;

    public BTreeSet(TKey sentinel, IPageManager<TKey> pageManager)
    {
        Root = pageManager.CreatePage(true);
        Add(new KeyPtr<TKey>(sentinel, int.MinValue));
        this.pageManager = pageManager;
    }

    public Page<TKey> Root { get; set; }

    public void Add(Page<TKey> page, KeyPtr<TKey> kp)
    {
        if (page.IsExternal)
        {
            page.Add(kp);
            return;
        }
        Page<TKey> next = page.Next(kp.Key);
        Add(next, kp);
        if (next.IsFull)
        {
            page.Add(next.Split());
        }
        //next.Close();
    }

    public void Add(KeyPtr<TKey> kp)
    {
        Add(Root, kp);
        // Root may have been filled by this add operation
        // check whether we need to split it
        if (Root.IsFull)
        {
            Page<TKey> left = Root;
            Page<TKey> right = Root.Split();
            Root = pageManager.CreatePage(false);
            Root.Add(left);
            Root.Add(right);
        }
    }

    public bool Contains(TKey key)
    {
        return Contains(Root, key);
    }

    public bool Contains(Page<TKey> page, TKey key)
    {
        if (page.IsExternal) return page.Contains(key);
        return Contains(page.Next(key), key);
    }
}