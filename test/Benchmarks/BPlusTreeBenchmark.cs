namespace Benchmarks;

#region

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Diagnosers;
using IndustrialInference.BPlusTree;

#endregion

[SimpleJob(RuntimeMoniker.Net90, baseline: true)]
[MemoryDiagnoser]
public class BPlusTreeBenchmark
{
    [Params(1, 2, 3, 5, 10)]
    public int ElementsToInsertShift { get; set; }
    private Random random = null!;

    [GlobalSetup]
    public void Setup() => random = new Random(29);

    [GlobalCleanup]
    public void Teardown()
    {
        //sut.Dispose();
    }

    [Benchmark]
    public void AddElements()
    {
        var sut = new BPlusTree<long, long>();
        for (var i = 0; i < 1 << ElementsToInsertShift; i++)
        {
            sut.Insert(i, random.Next(1, int.MaxValue));
        }
    }
}
