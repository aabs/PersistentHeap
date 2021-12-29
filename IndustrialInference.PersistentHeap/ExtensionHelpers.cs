namespace IndustrialInference.BPlusTree;

public static class ExtensionHelpers
{
    public static int BinarySearch<T, K>(this T[] buf, int end, K target, Func<T, K> selector)
    {
        var lo = 0;
        var hi = end;
        var comp = Comparer<K>.Default;

        while (lo <= hi)
        {
            var median = lo + (hi - lo >> 1);
            var num = comp.Compare(selector(buf[median]), target);
            if (num == 0)
                return median;
            if (num < 0)
                lo = median + 1;
            else
                hi = median - 1;
        }

        return ~lo;
    }

    public static int BinarySearch<T, K>(this T[] buf, K target, Func<T, K> selector)
    => BinarySearch(buf, buf.Length - 1, target, selector);
}