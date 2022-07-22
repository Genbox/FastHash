using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Genbox.FastHash.Benchmarks.Code;
using Genbox.FastHash.DJBHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FarshHash;
using Genbox.FastHash.FNVHash;
using Genbox.FastHash.Marvin;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.SipHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.xxHash;

namespace Genbox.FastHash.Benchmarks;

[MbPrSecColumn]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[InProcess]
public class HashBenchmark : IDisposable
{
    [Params(16, 32, 1024)]
    public int Size { get; set; }

    private readonly Random _rng = new Random(42);
    private byte[] _testData = null!;
    private unsafe byte* _ptr;

    [GlobalSetup]
    public unsafe void Setup()
    {
        _testData = GetRandomBytes(Size);
        _ptr = (byte*)NativeMemory.Alloc((nuint)Size);

        for (int i = 0; i < _testData.Length; i++)
        {
            _ptr[i] = _testData[i];
        }
    }

    [Benchmark]
    public uint DJBHash32Test() => DJBHash32.ComputeHash(_testData);

    [Benchmark]
    public uint FarmHash32Test() => FarmHash32.ComputeHash(_testData);

    [Benchmark]
    public ulong FarmHash64Test() => FarmHash64.ComputeHash(_testData);

    [Benchmark]
    public ulong FarshHash64Test() => FarshHash64.ComputeHash(_testData);

    [Benchmark]
    public uint FNV1A32Test() => FNV1A32.ComputeHash(_testData);

    [Benchmark]
    public ulong FNV1A64Test() => FNV1A64.ComputeHash(_testData);

    [Benchmark]
    public uint Marvin32Test() => Marvin32.ComputeHash(_testData, (uint)_testData.Length, 43);

    [Benchmark]
    public uint MurmurHash32Test() => MurmurHash32.ComputeHash(_testData);

    [Benchmark]
    public Uint128 MurmurHash128Test() => MurmurHash128.ComputeHash(_testData);

    [Benchmark]
    public ulong SipHash64Test() => SipHash64.ComputeHash(_testData);

    [Benchmark]
    public uint SuperFastHash32Test() => SuperFastHash32.ComputeHash(_testData);

    [Benchmark]
    public uint xx2Hash32Test() => xx2Hash32.ComputeHash(_testData);

    [Benchmark]
    public ulong xx2Hash64Test() => xx2Hash64.ComputeHash(_testData);

    [Benchmark]
    public unsafe uint DJBHash32UnsafeTest() => DJBHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe uint FarmHash32UnsafeTest() => FarmHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong FarshHash64UnsafeTest() => FarshHash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe uint FNV1A32UnsafeTest() => FNV1A32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong FNV1A64UnsafeTest() => FNV1A64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong FNV1AWHIZ32UnsafeTest() => FNV1AWHIZ32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong FNV1AYoshimitsuTRIAD32UnsafeTest() => FNV1AYoshimitsuTRIAD32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe uint MurmurHash32UnsafeTest() => MurmurHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe Uint128 MurmurHash128UnsafeTest() => MurmurHash128Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong SipHash64UnsafeTest() => SipHash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe uint SuperFastHash32UnsafeTest() => SuperFastHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe uint xx2Hash32UnsafeTest() => xx2Hash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong xx2Hash64UnsafeTest() => xx2Hash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong xx3Hash64UnsafeTest() => xx3Hash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe Uint128 xx3Hash128UnsafeTest() => xx3Hash128Unsafe.ComputeHash(_ptr, _testData.Length);

    private byte[] GetRandomBytes(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Number of bytes cannot be negative.");

        byte[] bytes = GC.AllocateUninitializedArray<byte>(count);
        _rng.NextBytes(bytes);
        return bytes;
    }

    public unsafe void Dispose()
    {
        NativeMemory.Free(_ptr);
        GC.SuppressFinalize(this);
    }
}