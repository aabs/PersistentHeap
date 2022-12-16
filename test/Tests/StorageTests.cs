using IndustrialInference.PersistentHeap.Old;
using System;
using System.Linq;

namespace PersistentHeap.Tests;

[TestFixture]
public class StorageTests
{
    [Test]
    public void CanAccessRangeOfLocations()
    {
        const int size = 32;
        var sut = new ByteStorage(size);
        for (int i = 0; i < 32; i++)
        {
            sut[i] = Convert.ToByte(i);
        }
        var actual = sut[2..4];
        sut[2].Should().Be(0x2);
        sut[1..3] = actual;
        sut[2].Should().Be(0x3);
    }

    [Test]
    public void CanCreateByteStorage()
    {
        const int size = 1024;
        var sut = new ByteStorage(size);
        sut.Should().NotBeNull();
    }

    [Test]
    public void CanOverwriteUsingRangeNotation()
    {
        const int size = 32;
        var sut = new ByteStorage(size);
        for (int i = 0; i < 32; i++)
        {
            sut[i] = Convert.ToByte(i);
        }
        var actual = sut[20..^4]; // i.e. 20,21,22,23,24,25,26,27
        actual[0].Should().Be(20);
        actual.Length.Should().Be(8);
        sut[..] = actual;
        sut[0].Should().Be(20);
        sut[8].Should().Be(0x8);
    }

    [Test]
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

    [Test]
    public void StorageHasCorrectBuffer()
    {
        const int size = 1024;
        var sut = new ByteStorage(size);
        sut.Buf.Should().NotBeNull().And.HaveCount(size);
    }

    [Test]
    public void StorageIsAllInitialised()
    {
        const int size = 32;
        var sut = new ByteStorage(size);
        sut.Buf.All(b => b == 0x0).Should().BeTrue();
    }
}
