using System.Runtime.InteropServices;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Genbox.FastHash.AesniHash;
using Genbox.FastHash.Benchmarks.Code;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FarshHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.FoldHash;
using Genbox.FastHash.GxHash;
using Genbox.FastHash.MarvinHash;
using Genbox.FastHash.MeowHash;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.PolymurHash;
using Genbox.FastHash.RapidHash;
using Genbox.FastHash.SipHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Benchmarks;

[MbPrSecColumn, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class HashBenchmarks : IDisposable
{
    private unsafe byte* _ptr;
    private byte[] _testData = null!;

    [Params(4, 8, 32, 128, 1024)]
    public int Size { get; set; }

    public unsafe void Dispose()
    {
        NativeMemory.Free(_ptr);
        GC.SuppressFinalize(this);
    }

    [GlobalSetup]
    public unsafe void Setup()
    {
        _testData = BenchmarkHelper.GetRandomBytes(Size);
        _ptr = (byte*)NativeMemory.Alloc((nuint)Size);

        for (int i = 0; i < _testData.Length; i++)
            _ptr[i] = _testData[i];
    }

    [Benchmark, BenchmarkCategory("AesniHash64")]
    public ulong AesniHash64Test() => AesniHash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("AesniHash128")]
    public UInt128 AesniHash128Test() => AesniHash128.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("CityHash32")]
    public uint CityHash32Test() => CityHash32.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("CityHash64")]
    public ulong CityHash64Test() => CityHash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("CityHash128")]
    public UInt128 CityHash128Test() => CityHash128.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("Djb2Hash32")]
    public uint Djb2Hash32Test() => Djb2Hash32.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("Djb2Hash64")]
    public ulong Djb2Hash64Test() => Djb2Hash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("FarmHash32")]
    public uint FarmHash32Test() => FarmHash32.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("FarmHash64")]
    public ulong FarmHash64Test() => FarmHash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("FarshHash64")]
    public ulong FarshHash64Test() => FarshHash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("FoldHash64")]
    public ulong FoldHash64Test() => FoldHash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("FoldHashQuality64")]
    public ulong FoldHashQuality64Test() => FoldHashQuality64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("Fnv1aHash32")]
    public uint Fnv1aHash32Test() => Fnv1aHash32.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("Fnv1aHash64")]
    public ulong Fnv1aHash64Test() => Fnv1aHash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("GxHash32")]
    public uint GxHash32Test() => GxHash32.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("Gx2Hash32")]
    public uint Gx2Hash32Test() => Gx2Hash32.ComputeHash(_testData, new UInt128(0, 0));

    [Benchmark, BenchmarkCategory("GxHash64")]
    public ulong GxHash64Test() => GxHash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("Gx2Hash64")]
    public ulong Gx2Hash64Test() => Gx2Hash64.ComputeHash(_testData, new UInt128(0, 0));

    [Benchmark, BenchmarkCategory("GxHash128")]
    public UInt128 GxHash128Test() => GxHash128.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("Gx2Hash128")]
    public UInt128 Gx2Hash128Test() => Gx2Hash128.ComputeHash(_testData, new UInt128(0, 0));

    [Benchmark, BenchmarkCategory("MarvinHash32")]
    public uint MarvinHash32Test() => MarvinHash32.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("MarvinHash64")]
    public ulong MarvinHash64Test() => MarvinHash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("Murmur3Hash32")]
    public uint Murmur3Hash32Test() => Murmur3Hash32.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("Murmur3Hash128")]
    public UInt128 Murmur3Hash128Test() => Murmur3Hash128.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("Polymur2Hash64")]
    public ulong Polymur2Hash64Test() => Polymur2Hash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("RapidHash64")]
    public ulong RapidHash64Test() => RapidHash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("RapidHashMicro64")]
    public ulong RapidHashMicro64Test() => RapidHashMicro64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("RapidHashNano64")]
    public ulong RapidHashNano64Test() => RapidHashNano64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("SipHash64")]
    public ulong SipHash64Test() => SipHash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("SuperFastHash32")]
    public uint SuperFastHash32Test() => SuperFastHash32.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("Xx2Hash32")]
    public uint Xx2Hash32Test() => Xx2Hash32.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("Xx2Hash64")]
    public ulong Xx2Hash64Test() => Xx2Hash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("Wy3Hash64")]
    public ulong Wy3Hash64Test() => Wy3Hash64.ComputeHash(_testData);

    [Benchmark, BenchmarkCategory("CityHash32")]
    public unsafe uint CityHash32UnsafeTest() => CityHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("CityHash64")]
    public unsafe ulong CityHash64UnsafeTest() => CityHash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("CityHash128")]
    public unsafe UInt128 CityHash128UnsafeTest() => CityHash128Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Djb2Hash32")]
    public unsafe uint Djb2Hash32UnsafeTest() => Djb2Hash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Djb2Hash64")]
    public unsafe ulong Djb2Hash64UnsafeTest() => Djb2Hash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("FarmHash32")]
    public unsafe uint FarmHash32UnsafeTest() => FarmHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("FarmHash64")]
    public unsafe ulong FarmHash64UnsafeTest() => FarmHash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("FarshHash64")]
    public unsafe ulong FarshHash64UnsafeTest() => FarshHash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Fnv1aHash32")]
    public unsafe uint Fnv1aHash32UnsafeTest() => Fnv1aHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Fnv1aHash64")]
    public unsafe ulong Fnv1aHash64UnsafeTest() => Fnv1aHash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("MeowHash64")]
    public unsafe ulong MeowHash64UnsafeTest() => MeowHash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("MeowHash128")]
    public unsafe UInt128 MeowHash128UnsafeTest() => MeowHash128Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Murmur3Hash32")]
    public unsafe uint Murmur3Hash32UnsafeTest() => Murmur3Hash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Murmur3Hash128")]
    public unsafe UInt128 Murmur3Hash128UnsafeTest() => Murmur3Hash128Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("SipHash64")]
    public unsafe ulong SipHash64UnsafeTest() => SipHash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("SuperFastHash32")]
    public unsafe uint SuperFastHash32UnsafeTest() => SuperFastHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Xx2Hash32")]
    public unsafe uint Xx2Hash32UnsafeTest() => Xx2Hash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Xx2Hash64")]
    public unsafe ulong Xx2Hash64UnsafeTest() => Xx2Hash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Xx3Hash64")]
    public unsafe ulong Xx3Hash64UnsafeTest() => Xx3Hash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Xx3Hash128")]
    public unsafe UInt128 Xx3Hash128UnsafeTest() => Xx3Hash128Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark, BenchmarkCategory("Wy3Hash64")]
    public unsafe ulong Wy3Hash64UnsafeTest() => Wy3Hash64Unsafe.ComputeHash(_ptr, _testData.Length);
}
