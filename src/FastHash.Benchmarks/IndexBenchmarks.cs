using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Genbox.FastHash.Benchmarks.Code;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Benchmarks;

[MbPrSecColumn(8)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[InProcess]
public class IndexBenchmarks
{
    private uint _value = 12808241;

    [Benchmark]
    public uint Djb2Hash32Test() => Djb2Hash32.ComputeIndex(_value);

    [Benchmark]
    public uint Fnv1aHash32Test() => Fnv1aHash32.ComputeIndex(_value);

    [Benchmark]
    public ulong Fnv1aHash64Test() => Fnv1aHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Wy3Hash64Test() => Wy3Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Xx2Hash64Test() => Xx2Hash64.ComputeIndex(_value);
}