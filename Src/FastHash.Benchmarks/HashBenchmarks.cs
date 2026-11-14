using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Genbox.FastHash.Benchmarks.Code;
using Genbox.FastHash.TestShared;

namespace Genbox.FastHash.Benchmarks;

[MbPrSecColumn, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class HashBenchmarks
{
    private readonly ulong[] _crcResult = new ulong[4];
    private byte[] _testData = null!;

    [Params(8, 64, 1024, 32 * 1024 * 1024)]
    public int Size { get; set; }

    public static IEnumerable<Hash32Algorithm> Hash32Algorithms() => AlgorithmCatalog.Hash32Algorithms.Where(static x => x.IsSupported && x.Hash != null);

    public static IEnumerable<Hash64Algorithm> Hash64Algorithms() => AlgorithmCatalog.Hash64Algorithms.Where(static x => x.IsSupported && x.Hash != null);

    public static IEnumerable<Hash128Algorithm> Hash128Algorithms() => AlgorithmCatalog.Hash128Algorithms.Where(static x => x.IsSupported && x.Hash != null);

    public static IEnumerable<Hash256Algorithm> Hash256Algorithms() => AlgorithmCatalog.Hash256Algorithms.Where(static x => x.IsSupported && x.Hash != null);

    [GlobalSetup]
    public void Setup() => _testData = BenchmarkHelper.GetRandomBytes(Size);

    [Benchmark, BenchmarkCategory("Hash32")]
    [ArgumentsSource(nameof(Hash32Algorithms))]
    public uint Hash32(Hash32Algorithm algorithm) => algorithm.Hash!(_testData);

    [Benchmark, BenchmarkCategory("Hash64")]
    [ArgumentsSource(nameof(Hash64Algorithms))]
    public ulong Hash64(Hash64Algorithm algorithm) => algorithm.Hash!(_testData);

    [Benchmark, BenchmarkCategory("Hash128")]
    [ArgumentsSource(nameof(Hash128Algorithms))]
    public UInt128 Hash128(Hash128Algorithm algorithm) => algorithm.Hash!(_testData);

    [Benchmark, BenchmarkCategory("Hash256")]
    [ArgumentsSource(nameof(Hash256Algorithms))]
    public void Hash256(Hash256Algorithm algorithm) => algorithm.Hash!(_testData, _crcResult);
}