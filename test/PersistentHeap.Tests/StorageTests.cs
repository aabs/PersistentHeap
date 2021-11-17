using System.Linq;
using IndustrialInference.PersistentHeap;

namespace PersistentHeap.Tests;

[TestClass]
public class StorageTests
{
    [TestMethod]
    public void CanCreateByteStorage()
    {
        const int size = 1024;
        var sut = new ByteStorage(size);
        sut.Should().NotBeNull();
    }

    [TestMethod]
    public void StorageHasCorrectBuffer()
    {
        const int size = 1024;
        var sut = new ByteStorage(size);
        sut.Buf.Should().NotBeNull().And.HaveCount(size);
    }

    [TestMethod]
    public void StorageIsAllInitialised()
    {
        const int size = 32;
        var sut = new ByteStorage(size);
        sut.Buf.All(b => b == 0x0).Should().BeTrue();
    }

    [TestMethod]
    public void RoundtripGettingAndSetting()
    {
        const int size = 32;
        var sut = new ByteStorage(size);
        sut.Buf.All(b => b == 0x0).Should().BeTrue();
        sut[12].Should().Be(0x0);
        sut[12] = 0xFF;
        sut.Buf.All(b => b == 0x0).Should().BeFalse();
        sut[12].Should().Be(0xFF);
    }

    [TestMethod]
    public void CanAccessRangeOfLocations()
    {
        const int size = 32;
        var sut = new ByteStorage(size);
        var actual = sut[2..4];
    }
}