using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Genbox.FastHashesNet.DJBHash;
using Genbox.FastHashesNet.FarmHash;
using Genbox.FastHashesNet.FarshHash;
using Genbox.FastHashesNet.FNVHash;
using Genbox.FastHashesNet.MurmurHash;
using Genbox.FastHashesNet.SipHash;
using Genbox.FastHashesNet.SuperFastHash;
using Genbox.FastHashesNet.xxHash;

namespace Genbox.FastHashesNet.Benchmarks;

[RankColumn]
[InProcess]
public class HashBenchmark : IDisposable
{
    [Params( /*4,*/ 8 /*, 40, 1024, 1048576*/)]
    public int TestSize { get; set; }

    private readonly Random _rng = new Random(42);
    private readonly byte[] _testData;
    private unsafe readonly byte* _ptr;

    public unsafe HashBenchmark()
    {
        _testData = GetRandomBytes(TestSize);
        _ptr = (byte*)NativeMemory.Alloc((nuint)TestSize);
        Unsafe.Copy(_ptr, ref _testData);
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
    public uint MurmurHash32Test() => MurmurHash32.ComputeHash(_testData);

    [Benchmark]
    public byte[] MurmurHash128Test() => MurmurHash128.ComputeHash(_testData);

    [Benchmark]
    public ulong SipHash64Test() => SipHash64.ComputeHash(_testData);

    [Benchmark]
    public uint SuperFastHash32Test() => SuperFastHash32.ComputeHash(_testData);

    [Benchmark]
    public uint xxHash32Test() => xxHash32.ComputeHash(_testData);

    [Benchmark]
    public ulong xxHash64Test() => xxHash64.ComputeHash(_testData);

    [Benchmark]
    public unsafe uint DJBHash32UnsafeTest() => DJBHash32Unsafe.ComputeHash(_ptr, TestSize);

    [Benchmark]
    public unsafe uint Farmhash32UnsafeTest() => FarmHash32Unsafe.ComputeHash(_ptr, _testData.Length);

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
    public unsafe byte[] MurmurHash128UnsafeTest() => MurmurHash128Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong SipHash64UnsafeTest() => SipHash64Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe uint SuperFastHash32UnsafeTest() => SuperFastHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe uint xxHash32UnsafeTest() => xxHash32Unsafe.ComputeHash(_ptr, _testData.Length);

    [Benchmark]
    public unsafe ulong xxHash64UnsafeTest() => xxHash64Unsafe.ComputeHash(_ptr, _testData.Length);

    private byte[] GetRandomBytes(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Number of bytes cannot be negative.");

        byte[] bytes = new byte[count];
        _rng.NextBytes(bytes);
        return bytes;
    }

    public unsafe void Dispose()
    {
        NativeMemory.Free(_ptr);
        GC.SuppressFinalize(this);
    }
}