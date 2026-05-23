using BenchmarkDotNet.Order;
using Genbox.FastHash.AesniHash;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FarshHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.FoldHash;
using Genbox.FastHash.GxHash;
using Genbox.FastHash.HighwayHash;
using Genbox.FastHash.MarvinHash;
using Genbox.FastHash.MeowHash;
using Genbox.FastHash.PolymurHash;
using Genbox.FastHash.RapidHash;
using Genbox.FastHash.SipHash;
using Genbox.FastHash.T1haHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Benchmarks;

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Index64Benchmarks
{
    private readonly ulong _value = 1337; // Keep readonly and non-static. Otherwise, compiler will inline and skew results.

    [Benchmark]
    public ulong AesniHash64Test() => AesniHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong CityHash64Test() => CityHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Djb2AltHash64Test() => Djb2AltHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Djb2Hash64Test() => Djb2Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong FarshHash64Test() => FarshHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong FarmHash64Test() => FarmHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong FoldHash64Test() => FoldHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong FoldHashQuality64Test() => FoldHashQuality64.ComputeIndex(_value);

    [Benchmark]
    public ulong Fnv1aHash64Test() => Fnv1aHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Gx2Hash64Test() => Gx2Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong HighwayHash64Test() => HighwayHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong MarvinHash64Test() => MarvinHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong MeowHash64Test() => MeowHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Polymur2Hash64Test() => Polymur2Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Rapid3Hash64Test() => Rapid3Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Rapid3HashMicro64Test() => Rapid3HashMicro64.ComputeIndex(_value);

    [Benchmark]
    public ulong Rapid3HashNano64Test() => Rapid3HashNano64.ComputeIndex(_value);

    [Benchmark]
    public ulong SipHash64Test() => SipHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong T1ha2Hash64Test() => T1ha2Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Wy3Hash64Test() => Wy3Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Wy4Hash64Test() => Wy4Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong XxHash64Test() => XxHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Xx3Hash64Test() => Xx3Hash64.ComputeIndex(_value);
}