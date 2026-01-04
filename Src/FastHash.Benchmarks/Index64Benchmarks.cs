using BenchmarkDotNet.Order;
using Genbox.FastHash.Benchmarks.Code;
using Genbox.FastHash.AesniHash;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FarshHash;
using Genbox.FastHash.FoldHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.GxHash;
using Genbox.FastHash.HighwayHash;
using Genbox.FastHash.MeowHash;
using Genbox.FastHash.MarvinHash;
using Genbox.FastHash.PolymurHash;
using Genbox.FastHash.RapidHash;
using Genbox.FastHash.SipHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Benchmarks;

[MbPrSecColumn(8)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Index64Benchmarks
{
    private readonly ulong _value = 12808224424451380151UL;
    private readonly Genbox.FastHash.UInt128 _gxSeed = new Genbox.FastHash.UInt128(0, 0);

    [Benchmark]
    public ulong AesniHash64Test() => AesniHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong CityHash64Test() => CityHash64.ComputeIndex(_value);

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
    public ulong GxHash64Test() => GxHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Gx2Hash64Test() => Gx2Hash64.ComputeIndex(_value, _gxSeed);

    [Benchmark]
    public ulong HighwayHash64UnsafeTest() => HighwayHash64Unsafe.ComputeIndex(_value);

    [Benchmark]
    public ulong MarvinHash64Test() => MarvinHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong MeowHash64UnsafeTest() => MeowHash64Unsafe.ComputeIndex(_value);

    [Benchmark]
    public ulong Polymur2Hash64Test() => Polymur2Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong RapidHash64Test() => RapidHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong RapidHashMicro64Test() => RapidHashMicro64.ComputeIndex(_value);

    [Benchmark]
    public ulong RapidHashNano64Test() => RapidHashNano64.ComputeIndex(_value);

    [Benchmark]
    public ulong SipHash64Test() => SipHash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Wy3Hash64Test() => Wy3Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Xx2Hash64Test() => Xx2Hash64.ComputeIndex(_value);

    [Benchmark]
    public ulong Xx3Hash64Test() => Xx3Hash64.ComputeIndex(_value);
}
