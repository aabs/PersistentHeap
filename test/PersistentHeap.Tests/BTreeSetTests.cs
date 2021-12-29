namespace PersistentHeap.Tests;

[TestClass]
public class BTreeSetTests
{
    [TestMethod]
    public void AddOneElement()
    {
        var pm = new PageManager<int>();
        var sut = new BTreeSet<int>(int.MinValue, pm);
        sut.Should().NotBeNull();
        const int unexpected = 13;
        const int expected = 17;
        sut.Add(new KeyPtr<int>(expected, default));
        sut.Contains(expected).Should().BeTrue();
        sut.Contains(unexpected).Should().BeFalse();

        sut.Add(new KeyPtr<int>(unexpected, default));
        sut.Contains(expected).Should().BeTrue();
        sut.Contains(unexpected).Should().BeTrue();

        sut.Add(new KeyPtr<int>(23, default));
        sut.Add(new KeyPtr<int>(44, default));
        sut.Add(new KeyPtr<int>(44, default));
    }
}