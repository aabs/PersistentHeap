namespace Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using IndustrialInference.BPlusTree;
using LightningDB;
using StructPacker;

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[RPlotExporter]
public class BPlusTreeBenchmark
{
    private BTreeSet<int> sut;
    private Random r;

    [Params(1, 2, 3, 5, 10, 15, 20)]
#pragma warning disable IDE1006 // Naming Styles
    public int N;
#pragma warning restore IDE1006 // Naming Styles

    [GlobalSetup]
    public void Setup()
    {
        var opts = new PageOptions { AllowDuplicates = false, PageSize = 1 << 10 };
        var pm = new PageManager<int>(opts);
        r = new Random(29);
        sut = new BTreeSet<int>(int.MinValue, pm);
    }

    [GlobalCleanup]
    public void Teardown()
    {
        //sut.Dispose();
    }

    [Benchmark]
    public void add_1_million_elements()
    {
        for (var i = 0; i < 1 << N; i++)
        {
            sut.Add(new KeyPtr<int>(r.Next(1, int.MaxValue), default));
        }
    }

    // Convert a byte array to an Object
    [Pack]
    public struct ContentsBlock
    {
        public ContentsBlock(){}
        public ContentsBlock(int version, int soIndexBlockName)
        {
            Version = version;
            SoIndexBlockName = soIndexBlockName;
        }
        public int Version { get; set; }
        public int SoIndexBlockName { get; set; }
    }
}
