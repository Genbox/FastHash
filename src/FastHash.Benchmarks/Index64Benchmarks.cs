using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Genbox.FastHash.Benchmarks.Code;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.MarvinHash;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Benchmarks;

[MbPrSecColumn(8)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Index64Benchmarks
{
    private readonly ulong _value = 12808224424451380151UL;

    [Benchmark]
    public ulong CityHash64Test() => CityHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Djb2Hash64Test() => Djb2Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Fnv1aHash64Test() => Fnv1aHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Wy3Hash64Test() => Wy3Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Xx2Hash64Test() => Xx2Hash64.ComputeIndex(_value);
}