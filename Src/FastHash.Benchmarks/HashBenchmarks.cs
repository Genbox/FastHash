using System.Runtime.InteropServices;
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
    private unsafe byte* _ptr;
    private byte[] _testData = null!;

    [Params(32 * 1024 * 1024)]
    public int Size { get; set; }

    public static IEnumerable<Hash32Algorithm> ManagedHash32Algorithms() => AlgorithmCatalog.Hash32Algorithms.Where(static x => x.Hash != null);

    public static IEnumerable<Hash64Algorithm> ManagedHash64Algorithms() => AlgorithmCatalog.Hash64Algorithms.Where(static x => x.Hash != null);

    public static IEnumerable<Hash128Algorithm> ManagedHash128Algorithms() => AlgorithmCatalog.Hash128Algorithms.Where(static x => x.Hash != null);

    public static IEnumerable<Hash256Algorithm> ManagedHash256Algorithms() => AlgorithmCatalog.Hash256Algorithms.Where(static x => x.Hash != null);

    public static IEnumerable<Hash32Algorithm> UnsafeHash32Algorithms() => AlgorithmCatalog.Hash32Algorithms.Where(static x => x.UnsafeHash != null);

    public static IEnumerable<Hash64Algorithm> UnsafeHash64Algorithms() => AlgorithmCatalog.Hash64Algorithms.Where(static x => x.UnsafeHash != null);

    public static IEnumerable<Hash128Algorithm> UnsafeHash128Algorithms() => AlgorithmCatalog.Hash128Algorithms.Where(static x => x.UnsafeHash != null);

    public static IEnumerable<Hash256Algorithm> UnsafeHash256Algorithms() => AlgorithmCatalog.Hash256Algorithms.Where(static x => x.UnsafeHash != null);

    [GlobalSetup]
    public unsafe void Setup()
    {
        Cleanup();

        _testData = BenchmarkHelper.GetRandomBytes(Size);
        _ptr = (byte*)NativeMemory.Alloc((nuint)Size);

        for (int i = 0; i < _testData.Length; i++)
            _ptr[i] = _testData[i];
    }

    [GlobalCleanup]
    public unsafe void Cleanup()
    {
        if (_ptr == null)
            return;

        NativeMemory.Free(_ptr);
        _ptr = null;
    }

    [Benchmark, BenchmarkCategory("Managed32")]
    [ArgumentsSource(nameof(ManagedHash32Algorithms))]
    public uint Managed32(Hash32Algorithm algorithm) => algorithm.Hash!(_testData);

    [Benchmark, BenchmarkCategory("Managed64")]
    [ArgumentsSource(nameof(ManagedHash64Algorithms))]
    public ulong Managed64(Hash64Algorithm algorithm) => algorithm.Hash!(_testData);

    [Benchmark, BenchmarkCategory("Managed128")]
    [ArgumentsSource(nameof(ManagedHash128Algorithms))]
    public UInt128 Managed128(Hash128Algorithm algorithm) => algorithm.Hash!(_testData);

    [Benchmark, BenchmarkCategory("Managed256")]
    [ArgumentsSource(nameof(ManagedHash256Algorithms))]
    public void Managed256(Hash256Algorithm algorithm) => algorithm.Hash!(_testData, _crcResult);

    [Benchmark, BenchmarkCategory("Unsafe32")]
    [ArgumentsSource(nameof(UnsafeHash32Algorithms))]
    public unsafe uint Unsafe32(Hash32Algorithm algorithm) => algorithm.UnsafeHash!(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Unsafe64")]
    [ArgumentsSource(nameof(UnsafeHash64Algorithms))]
    public unsafe ulong Unsafe64(Hash64Algorithm algorithm) => algorithm.UnsafeHash!(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Unsafe128")]
    [ArgumentsSource(nameof(UnsafeHash128Algorithms))]
    public unsafe UInt128 Unsafe128(Hash128Algorithm algorithm) => algorithm.UnsafeHash!(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Unsafe256")]
    [ArgumentsSource(nameof(UnsafeHash256Algorithms))]
    public unsafe void Unsafe256(Hash256Algorithm algorithm)
    {
        fixed (ulong* result = _crcResult)
            algorithm.UnsafeHash!(_ptr, _testData.Length, result);
    }
}