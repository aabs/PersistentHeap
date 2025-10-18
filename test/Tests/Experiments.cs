namespace PersistentHeap.Tests;

using IndustrialInference.PersistentHeap.Old;
using LightningDB;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;

using StructPacker;
using Random = System.Random;

public class Experiments
{
    [Fact]
    public void ArraySegmentTest()
    {
        var buf = new Memory<ulong>(new ulong[32]);
    }

    [Fact]
    public void CreateAndUseLightningDb()
    {
        using (var env = new LightningEnvironment("pathtofolder"))
        {
            env.MaxDatabases = 2;
            env.Open();

            using (var tx = env.BeginTransaction())
            using (var db = tx.OpenDatabase("custom", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                tx.Put(db, Encoding.UTF8.GetBytes("hello"), Encoding.UTF8.GetBytes("world"));
                tx.Commit();
            }
            using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
            using (var db = tx.OpenDatabase("custom"))
            {
                var (resultCode, key, value) = tx.Get(db, Encoding.UTF8.GetBytes("hello"));
                value.CopyToNewArray().Should().Equal(Encoding.UTF8.GetBytes("world"));
            }
        }
    }

    [Fact]
    public void GetAndSetPageViaLDB()
    {
        var r = new Random(29);
        var contents1 = new ContentsBlock(r.Next(), r.Next());
        using var env = new LightningEnvironment("pathtofolder");
        env.MaxDatabases = 2;
        env.Open();
        // write
        {
            using var tx = env.BeginTransaction();
            using var db = tx.OpenDatabase(null, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            var arr = new ReadOnlySpan<ContentsBlock>(new[] { contents1 });
            var arrSpan = MemoryMarshal.Cast<ContentsBlock, byte>(arr);
            var keySpan = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes("Contents"));
            tx.Put(db, keySpan, arrSpan);
            tx.Commit();
        }
        // read back
        {
            using var tx = env.BeginTransaction();
            using var db = tx.OpenDatabase(null, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            var (resultCode, key, value) = tx.Get(db, Encoding.UTF8.GetBytes("Contents"));
            tx.Commit();
            var contentSpan = MemoryMarshal.Cast<byte, ContentsBlock>(value.AsSpan());
            var contents2 = contentSpan.ToArray()[0];
            contents1.Should().Be(contents2);
        }
    }

    [Fact]
    public void MemoryOverArrayOfBytesTest()
    {
        var buf = new byte[1024];
        var sut = new Memory<byte>(buf, 0, 8);
        var span = sut.Span;
        span[0] = 0x1;
        span[1] = 0x2;
        buf[0].Should().Be(0x1);
        buf[1].Should().Be(0x2);
    }

    [Fact]
    public void TestCreationAndCastingOfMemorys()
    {
        Memory<byte> span = new byte[1024];
        var slice = span.Slice(0, 4 * sizeof(int)); // grab a span allowing access to four ints
        var sliceint = MemoryMarshal.Cast<byte, int>(slice.Span);
        var r = new Random();
        for (var i = 0; i < sliceint.Length; i++)
            sliceint[i] = r.Next();
        var sliceint2 = MemoryMarshal.Cast<byte, int>(span.Span);
        foreach (var j in sliceint2)
            Console.WriteLine(j);
    }

    [Fact]
    public void TestCreationAndCastingOfSpans()
    {
        Span<byte> span = stackalloc byte[1024];
        var slice = span.Slice(0, 4 * sizeof(int)); // grab a span allowing access to four ints
        var sliceint = MemoryMarshal.Cast<byte, int>(slice);
        var r = new Random();
        for (var i = 0; i < sliceint.Length; i++)
            sliceint[i] = r.Next();
        var sliceint2 = MemoryMarshal.Cast<byte, int>(span);
        foreach (var j in sliceint2)
            Console.WriteLine(j);
    }

    [Fact]
    public void TestFileExpansionTest()
    {
        var r = new Random();
        var tmpFilePath = Path.GetTempFileName();
        Console.WriteLine(tmpFilePath);
        using (var f = File.OpenWrite(tmpFilePath))
        {
            Memory<byte> buf = new byte[1024];
            var sliceint = MemoryMarshal.Cast<byte, int>(buf.Span);
            for (var i = 0; i < sliceint.Length; i++)
                sliceint[i] = r.Next();
            f.Write(buf.Span);
        }

        using (var f = File.Open(tmpFilePath, FileMode.Append))
        {
            Memory<byte> buf = new byte[2048];
            var sliceint = MemoryMarshal.Cast<byte, int>(buf.Span);
            for (var i = 0; i < sliceint.Length; i++)
                sliceint[i] = r.Next();
            f.Write(buf.Span);
        }

        // https://github.com/mgravell/Pipelines.Sockets.Unofficial/blob/master/src/Pipelines.Sockets.Unofficial/UnsafeMemory.cs
        using (var mmf = MemoryMappedFile.CreateFromFile(tmpFilePath, FileMode.Open))
        {
            unsafe
            {
                using var accessor = mmf.CreateViewAccessor(0, 256);
                byte* ptr = null;
                accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                using var ummm = new UnmanagedMemoryManager<byte>(ptr, 256);
                var span = ummm.GetSpan();
                var sliceint2 = MemoryMarshal.Cast<byte, int>(span);
                foreach (var j in sliceint2)
                    Console.WriteLine(j);
            }
        }
    }

    [Pack]
    public struct ContentsBlock
    {
        public ContentsBlock(int version, int soIndexBlockName)
        {
            Version = version;
            SoIndexBlockName = soIndexBlockName;
        }

        public int SoIndexBlockName { get; set; }
        public int Version { get; set; }
    }
}
