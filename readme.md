# Persistent Heap

## Benchmark Summary for BTreeSet Operations

```
BenchmarkDotNet v0.13.12, Windows 10 (10.0.19044.4291/21H2/November2021Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.300-preview.24203.14
  [Host]   : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0
```

| Method                 | N  | Mean             | Error            | StdDev           | Median           | Ratio |
|----------------------- |--- |-----------------:|-----------------:|-----------------:|-----------------:|------:|
| add_1_million_elements | 1  |         29.36 us |         0.766 us |         2.222 us |         28.51 us |  1.00 |
|                        |    |                  |                  |                  |                  |       |
| add_1_million_elements | 2  |         66.84 us |         3.281 us |         9.467 us |         64.30 us |  1.00 |
|                        |    |                  |                  |                  |                  |       |
| add_1_million_elements | 3  |        143.36 us |         9.078 us |        25.901 us |        132.74 us |  1.00 |
|                        |    |                  |                  |                  |                  |       |
| add_1_million_elements | 5  |        490.71 us |        14.329 us |        41.111 us |        483.94 us |  1.00 |
|                        |    |                  |                  |                  |                  |       |
| add_1_million_elements | 10 |     17,253.94 us |       497.656 us |     1,419.842 us |     16,881.17 us |  1.00 |
|                        |    |                  |                  |                  |                  |       |
| add_1_million_elements | 15 |    547,877.60 us |    13,566.930 us |    39,143.696 us |    535,368.30 us |  1.00 |
|                        |    |                  |                  |                  |                  |       |
| add_1_million_elements | 20 | 20,218,700.49 us | 1,413,363.820 us | 3,916,424.488 us | 19,063,342.50 us |  1.00 |
