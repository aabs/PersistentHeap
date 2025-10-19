namespace Benchmarks;

using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LightningDB;
using StructPacker;

[SimpleJob(RuntimeMoniker.Net60, baseline: true)]
[RPlotExporter]
public class LightningBenchmark
{
    private LightningEnvironment env;
    private Random r;

    [Params(1, 10, 100)]
#pragma warning disable IDE1006 // Naming Styles
    public int N;
#pragma warning restore IDE1006 // Naming Styles

    private LightningTransaction tx;
    private LightningDatabase db;

    [GlobalSetup]
    public void Setup()
    {
        env = new LightningEnvironment("pathtofolder");
        env.MaxDatabases = 2;
        env.Open();
        r = new Random(29);
        tx = env.BeginTransaction();
        db = tx.OpenDatabase(null, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
    }

    [GlobalCleanup]
    public void Teardown()
    {
        tx.TruncateDatabase(db);
        tx.Commit();
        db.Dispose();
        tx.Dispose();
        env.Dispose();
    }

    [Benchmark]
    public void GetAndSetPageViaSpans()
    {
        var contents1 = new ContentsBlock(r.Next(), r.Next());
        // write
        {
            for (int i = 0; i < N; i++)
            {
                var arr = new ReadOnlySpan<ContentsBlock>(new[] { contents1 });
                var arrSpan = MemoryMarshal.Cast<ContentsBlock, byte>(arr);
                var keySpan = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes($"Contents {i}"));
                tx.Put(db, keySpan, arrSpan);
            }
        }
        // read back
        {
            for (int i = 0; i < N; i++)
            {
                var keySpan = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes($"Contents {i}"));
                var (resultCode, key, value) = tx.Get(db, keySpan);

                var contentSpan = MemoryMarshal.Cast<byte, ContentsBlock>(value.AsSpan());
                var contents2 = contentSpan.ToArray()[0];
            }
        }
    }


    [Benchmark]
    public void GetAndSetPageViaArrays()
    {
        var contents1 = new ContentsBlock(r.Next(), r.Next());
        // write
        {
            for (int i = 0; i < N; i++)
            {
                var keySpan = Encoding.UTF8.GetBytes($"Contents {i}");
                tx.Put(db, keySpan, contents1.Pack());
            }
        }
        // read back
        {
            for (int i = 0; i < N; i++)
            {
                var keySpan = Encoding.UTF8.GetBytes($"Contents {i}");
                var (resultCode, key, value) = tx.Get(db, keySpan);
                ContentsBlock contents2 = new();
                contents2.Unpack(value.CopyToNewArray());
            }
        }
    }

    // Convert a byte array to an Object
    [Pack]
    public struct ContentsBlock
    {
        public ContentsBlock() { }
        public ContentsBlock(int version, int soIndexBlockName)
        {
            Version = version;
            SoIndexBlockName = soIndexBlockName;
        }
        public int Version { get; set; }
        public int SoIndexBlockName { get; set; }
    }
}
