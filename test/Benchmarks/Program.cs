using BenchmarkDotNet.Running;
using Benchmarks;

// Usage:
//   dotnet run -c Release --project test/Benchmarks -- bptree
//   dotnet run -c Release --project test/Benchmarks -- lightning   (requires RUN_LMDB_BENCHMARKS env var)

var which = args.Length > 0 ? args[0].ToLowerInvariant() : "bptree";
switch (which)
{
    case "bptree":
        BenchmarkRunner.Run<BPlusTreeBenchmark>();
        break;
    case "lightning":
        BenchmarkRunner.Run<LightningBenchmark>();
        break;
    default:
        Console.Error.WriteLine($"Unknown benchmark '{which}'. Use 'bptree' or 'lightning'.");
        Environment.ExitCode = 2;
        break;
}
