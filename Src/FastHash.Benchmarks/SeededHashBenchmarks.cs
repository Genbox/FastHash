using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Genbox.FastHash.Benchmarks.Code;
using Genbox.FastHash.ClHash;
using Genbox.FastHash.PolymurHash;

namespace Genbox.FastHash.Benchmarks;

[MbPrSecColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class SeededHashBenchmarks
{
    private const ulong Seed1 = 0x0123456789ABCDEF;
    private const ulong Seed2 = 0xFEDCBA9876543210;
    private ulong[] _clHashKey = null!;

    private byte[] _data = null!;
    private PolymurHashParams _polymurParameters = null!;

    [Params(8, 64, 1024)]
    public int Size { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _data = BenchmarkHelper.GetRandomBytes(Size);
        _clHashKey = ClHash64.CreateKey(Seed1, Seed2);
        _polymurParameters = new PolymurHashParams(Seed1);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("CLHash")]
    public ulong ClHashReuse() => ClHash64.ComputeHash(_data, _clHashKey);

    [Benchmark]
    [BenchmarkCategory("CLHash")]
    public ulong ClHashSeededSetup() => ClHash64.ComputeHash(_data, Seed1, Seed2);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Polymur")]
    public ulong PolymurReuse() => Polymur2Hash64.ComputeHash(_data, _polymurParameters);

    [Benchmark]
    [BenchmarkCategory("Polymur")]
    public ulong PolymurSeededSetup() => Polymur2Hash64.ComputeHash(_data, Seed1);
}