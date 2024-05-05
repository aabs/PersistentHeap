namespace PersistentHeap.Tests;

#region

using System.ComponentModel;
using System.Diagnostics;
using Random = System.Random;

#endregion


[Category("B+Tree")]
public class BTreeSetTests
{
    public BTreeSetTests()
    {
        var opts = new PageOptions { AllowDuplicates = false, PageSize = 4 };
        PageManager = new PageManager<int>(opts);
    }

    public PageManager<int> PageManager { get; set; }

    [Category("Slow")]
    [Fact(Skip = "too slow")]
    public void CanAddLargeNumbersOfElements()
    {
        var sw = new Stopwatch();
        sw.Start();
        var opts = new PageOptions { AllowDuplicates = false, PageSize = 1 << 10 };
        var pm = new PageManager<int>(opts);
        var r = new Random(13);
        var sut = new BTreeSet<int>(int.MinValue, pm);
        for (var i = 0; i < 1 << 20; i++)
        {
            sut.Add(new KeyPtr<int>(r.Next(1, int.MaxValue), default));
        }

        sw.Stop();
        pm.Body.Should().HaveCount(2051);
        Console.WriteLine($"time: {sw.Elapsed:t}");
    }

    [Fact]
    public void CanAddOne()
    {
        var sut = new BTreeSet<int>(int.MinValue, PageManager);
        sut.Should().NotBeNull();
        const int expected = 17;
        const int unexpected = 13;
        sut.Add(new KeyPtr<int>(expected, default));
        sut.Contains(expected).Should().BeTrue();
        sut.Contains(unexpected).Should().BeFalse();
    }

    [Fact]
    public void CanCheckEmptyTreeForContains()
    {
        var sut = new BTreeSet<int>(int.MinValue, PageManager);
        sut.Contains(23).Should().BeFalse();
    }

    [Fact]
    public void CanCheckForPresentElement()
    {
        var sut = new BTreeSet<int>(int.MinValue, PageManager);
        sut.Contains(23).Should().BeFalse();
        sut.Add(new KeyPtr<int>(23, default));
        sut.Contains(23).Should().BeTrue();
    }

    [Fact]
    public void CanSplitOnOverflow()
    {
        var sut = new BTreeSet<int>(int.MinValue, PageManager);
        sut.Should().NotBeNull();
        sut.Add(new KeyPtr<int>(17, default));
        sut.Add(new KeyPtr<int>(13, default));
        sut.Contains(17).Should().BeTrue();
        sut.Contains(13).Should().BeTrue();

        sut.Add(new KeyPtr<int>(23, default));
        sut.Add(new KeyPtr<int>(44, default));
        sut.Add(new KeyPtr<int>(44, default));
    }
}
