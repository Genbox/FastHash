using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Genbox.FastHash.Benchmarks.Code;
using Genbox.FastHash.DJBHash;
using Genbox.FastHash.FNVHash;
using Genbox.FastHash.wyHash;
using Genbox.FastHash.xxHash;

namespace Genbox.FastHash.Benchmarks;

[MbPrSecColumn(8)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[InProcess]
public class IndexBenchmarks
{
    private uint _value = 12808241;

    [Benchmark]
    public uint DJB2Hash32Test() => Djb2Hash32.ComputeIndex(_value);

    [Benchmark]
    public uint FNV1a32Test() => Fnv1aHash32.ComputeIndex(_value);

    [Benchmark]
    public ulong FNV1a64Test() => Fnv1aHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong wy3Hash64Test() => Wy3Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong xx2Hash64Test() => Xx2Hash64.ComputeIndex(_value);
}