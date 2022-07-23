using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Genbox.FastHash.Benchmarks.Code;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FarshHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.MarvinHash;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.SipHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Benchmarks;

[MbPrSecColumn]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[InProcess]
public class HashBenchmarks : IDisposable
{
    private readonly Random _rng = new Random(42);
    private unsafe byte* _ptr;
    private byte[] _testData = null!;

    [Params(8, 32, 128, 1024, 1024 * 1024 * 32)]
    public int Size { get; set; }

    public unsafe void Dispose()
    {
        NativeMemory.Free(_ptr);
        GC.SuppressFinalize(this);
    }

    [GlobalSetup]
    public unsafe void Setup()
    {
        _testData = GetRandomBytes(Size);
        _ptr = (byte*)NativeMemory.Alloc((nuint)Size);

        for (int i = 0; i < _testData.Length; i++)
            _ptr[i] = _testData[i];
    }

    [Benchmark]
    public uint Djb2Hash32Test() => Djb2Hash32.ComputeHash(_testData);

    [Benchmark]
    public uint FarmHash32Test() => FarmHash32.ComputeHash(_testData);

    [Benchmark]
    public ulong FarmHash64Test() => FarmHash64.ComputeHash(_testData);

    [Benchmark]
    public ulong FarshHash64Test() => FarshHash64.ComputeHash(_testData);

    [Benchmark]
    public uint Fnv1aHash32Test() => Fnv1aHash32.ComputeHash(_testData);

    [Benchmark]
    public ulong Fnv1aHash64Test() => Fnv1aHash64.ComputeHash(_testData);

    [Benchmark]
    public uint MarvinHash32Test() => MarvinHash32.ComputeHash(_testData, (uint)_testData.Length, 43);

    [Benchmark]
    public uint Murmur3Hash32Test() => Murmur3Hash32.ComputeHash(_testData);

    [Benchmark]
    public Uint128 Murmur3Hash128Test() => Murmur3Hash128.ComputeHash(_testData);

    [Benchmark]
    public ulong SipHash64Test() => SipHash64.ComputeHash(_testData);

    [Benchmark]
    public uint SuperFastHash32Test() => SuperFastHash32.ComputeHash(_testData);

    [Benchmark]
    public uint Xx2Hash32Test() => Xx2Hash32.ComputeHash(_testData);

    [Benchmark]
    public ulong Xx2Hash64Test() => Xx2Hash64.ComputeHash(_testData);

    [Benchmark]
    public ulong Wy3Hash64Test() => Wy3Hash64.ComputeHash(_testData);

    [Benchmark]
    public unsafe uint Djb2Hash32UnsafeTest() => Djb2Hash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe uint FarmHash32UnsafeTest() => FarmHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong FarshHash64UnsafeTest() => FarshHash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe uint Fnv1aHash32UnsafeTest() => Fnv1aHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong Fnv1aHash64UnsafeTest() => Fnv1aHash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong Fnv1aYtHash32UnsafeTest() => Fnv1aYtHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe uint Murmur3Hash32UnsafeTest() => Murmur3Hash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe Uint128 Murmur3Hash128UnsafeTest() => Murmur3Hash128Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong SipHash64UnsafeTest() => SipHash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe uint SuperFastHash32UnsafeTest() => SuperFastHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe uint Xx2Hash32UnsafeTest() => Xx2Hash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong Xx2Hash64UnsafeTest() => Xx2Hash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong Xx3Hash64UnsafeTest() => Xx3Hash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe Uint128 Xx3Hash128UnsafeTest() => Xx3Hash128Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong Wy3Hash64UnsafeTest() => Wy3Hash64Unsafe.ComputeHash(_ptr, _testData.Length);

    private byte[] GetRandomBytes(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Number of bytes cannot be negative.");

        byte[] bytes = GC.AllocateUninitializedArray<byte>(count);
        _rng.NextBytes(bytes);
        return bytes;
    }
}