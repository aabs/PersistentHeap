using System.Diagnostics;
using System.Text;

namespace IndustrialInference.BPlusTree;

[DebuggerDisplay("{ToString(), nq}")]
public class Page<TKey> where TKey : IComparable<TKey>
{
    /// <summary>
    /// How much space in this page
    /// </summary>
    private readonly PageManager<TKey> pageManager;

    /// <summary>
    /// How many entries are currently in use
    /// </summary>
    private int Entries;

    /// <summary>
    /// Create the page, init storage, and start tracking usage
    /// </summary>
    /// <param name="isExternal">is this a leaf node (or is it a container of references to pages)</param>
    public Page(bool isExternal, int location, PageManager<TKey> pageManager, PageOptions? options)
    {
        IsExternal = isExternal;
        Location = location;
        this.pageManager = pageManager;
        Options = options ?? PageOptions.Default;
        Body = new KeyPtr<TKey>[Options.PageSize];
        Entries = 0;
    }

    /// <summary>
    /// Is the page a container for references (false) or for data (true)?
    /// </summary>
    public bool IsExternal { get; }

    public bool IsFull => Entries == Options.PageSize;

    /// <summary>
    /// Ket the keys of the entries in the page
    /// </summary>
    public IEnumerable<TKey> Keys
    {
        get
        {
            foreach (var kp in Body)
            {
                yield return kp.Key;
            }
        }
    }

    public int Location { get; set; }
    public PageOptions? Options { get; }

    /// <summary>
    /// The actual data stored in the page
    /// </summary>
    private KeyPtr<TKey>[] Body { get; init; }

    /// <summary>
    /// Add an entry to the page (in order)
    /// </summary>
    /// <param name="key">The key to be added</param>
    /// <exception cref="ApplicationException">if the page is full adding will fail</exception>
    public void Add(KeyPtr<TKey> key)
    {
        Debug.Assert(IsExternal);
        if (IsFull) throw new ApplicationException("cannot add entry to full page");
        if (Options.AllowDuplicates && Contains(key.Key))
        {
            return;
        }
        Body[Entries++] = key;
        Array.Sort(Body, 0, Entries,
            Comparer<KeyPtr<TKey>>.Create(
                (l, r) => Comparer<TKey>.Default.Compare(l.Key, r.Key)));
    }

    /// <summary>
    /// Add a page to an internal page
    /// </summary>
    /// <param name="page">The page to add</param>
    /// <exception cref="ApplicationException">if the page is full adding will fail</exception>
    public void Add(Page<TKey> page)
    {
        if (IsFull) throw new ApplicationException("cannot add page to full page");
        Debug.Assert(!IsExternal);
        var smallestKeyInPage = page.Body[0].Key;
        Body[Entries++] = new KeyPtr<TKey>(smallestKeyInPage, page.Location);
        Array.Sort(Body, 0, Entries,
            Comparer<KeyPtr<TKey>>.Create(
                (l, r) => Comparer<TKey>.Default.Compare(l.Key, r.Key)));
    }

    /// <summary>
    /// Write this page back to disk
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Close()
    {
        throw new NotImplementedException();
    }

    public bool Contains(TKey key)
    {
        var x = Body.BinarySearch(Entries, key, kp => kp.Key);
        if (x < 0) return false;
        return true;
        //for (int i = 0; i < Entries; i++)
        //{
        //    if (Cmp(Body[i].Key, key) == 0) return true;
        //}
        //return false;
    }

    public Page<TKey> Next(TKey key)
    {
        int i = Entries - 1;
        while (Cmp(Body[i].Key, key) > 0) i++;
        return pageManager.Lookup(Body[i].Ptr);
    }

    /// <summary>
    /// Destructive split that  takes the top half of this page and adds it to a new page (removing it from this page)
    /// </summary>
    /// <returns>A new page with the entries for the top half of this page</returns>
    public Page<TKey> Split()
    {
        var r = pageManager.CreatePage(IsExternal);
        var medianIndex = Body.Length / 2;
        Array.Copy(Body, medianIndex, r.Body, 0, medianIndex);
        r.Entries = medianIndex;
        Array.Clear(Body, medianIndex, Body.Length - medianIndex);
        Entries = medianIndex;
        return r;
    }

    public override string? ToString()
    {
        var sb = new StringBuilder();
        if (int.MinValue is TKey sentinel)
        {
            var sep = "";
            sb.Append('[');
            for (int i = 0; i < Entries; i++)
            {
                var kp = Body[i];
                sb.Append(sep); sep = " | ";
                sb.Append(kp);
            }
            sb.Append(']');
        }
        return sb.ToString();
    }

    private int Cmp(TKey l, TKey r)
    {
        return Comparer<TKey>.Default.Compare(l, r);
    }
}

public class PageOptions
{
    public static PageOptions Default => new PageOptions { AllowDuplicates = false, PageSize = 1 << 20 };
    public bool AllowDuplicates { get; set; }
    public int PageSize { get; set; }
}