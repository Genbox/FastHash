using System;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using FastHashesNet.DJBHash;
using FastHashesNet.FarmHash;
using FastHashesNet.FarshHash;
using FastHashesNet.FNVHash;
using FastHashesNet.MurmurHash;
using FastHashesNet.SipHash;
using FastHashesNet.SuperFastHash;
using FastHashesNet.xxHash;

namespace FastHashesNet.Benchmarks;

[RankColumn]
public sealed class HashBenchmark : IDisposable
{
    [Params(/*4,*/ 8/*, 40, 1024, 1048576*/)]
    public int TestSize { get; set; }

    private readonly Random _rng = new Random();
    private readonly byte[] _testData;

    private readonly IntPtr _ptr;
    private readonly unsafe byte* _unsafePtr;

    public unsafe HashBenchmark()
    {
        _testData = GetRandomBytes(TestSize);
        _ptr = Marshal.AllocHGlobal(TestSize);
        Marshal.Copy(_testData, 0, _ptr, _testData.Length);

        _unsafePtr = (byte*)_ptr.ToPointer();
    }

    [Benchmark]
    public uint DJBHash32Test()
    {
        return DJBHash32.ComputeHash(_testData);
    }

    [Benchmark]
    public uint FarmHash32Test()
    {
        return FarmHash32.ComputeHash(_testData);
    }

    [Benchmark]
    public ulong FarmHash64Test()
    {
        return FarmHash64.ComputeHash(_testData);
    }

    [Benchmark]
    public ulong FarshHash64Test()
    {
        return FarshHash64.ComputeHash(_testData);
    }

    [Benchmark]
    public uint FNV1A32Test()
    {
        return FNV1A32.ComputeHash(_testData);
    }

    [Benchmark]
    public ulong FNV1A64Test()
    {
        return FNV1A64.ComputeHash(_testData);
    }

    [Benchmark]
    public uint MurmurHash32Test()
    {
        return MurmurHash32.ComputeHash(_testData);
    }

    [Benchmark]
    public byte[] MurmurHash128Test()
    {
        return MurmurHash128.ComputeHash(_testData);
    }

    [Benchmark]
    public ulong SipHash64Test()
    {
        return SipHash64.ComputeHash(_testData);
    }
    [Benchmark]
    public uint SuperFastHash32Test()
    {
        return SuperFastHash32.ComputeHash(_testData);
    }

    [Benchmark]
    public uint xxHash32Test()
    {
        return xxHash32.ComputeHash(_testData);
    }

    [Benchmark]
    public ulong xxHash64Test()
    {
        return xxHash64.ComputeHash(_testData);
    }

    [Benchmark]
    public unsafe uint DJBHash32UnsafeTest()
    {
        return DJBHash32Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }

    [Benchmark]
    public unsafe uint Farmhash32UnsafeTest()
    {
        return FarmHash32Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }

    [Benchmark]
    public unsafe ulong Farmhash64UnsafeTest()
    {
        return FarmHash64Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }

    [Benchmark]
    public unsafe ulong FarshHash64UnsafeTest()
    {
        return FarshHash64Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }

    [Benchmark]
    public unsafe uint FNV1A32UnsafeTest()
    {
        return FNV1A32Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }

    [Benchmark]
    public unsafe ulong FNV1A64UnsafeTest()
    {
        return FNV1A64Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }

    [Benchmark]
    public unsafe ulong FNV1AWHIZ32UnsafeTest()
    {
        return FNV1AWHIZ32Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }

    [Benchmark]
    public unsafe ulong FNV1AYoshimitsuTRIAD32UnsafeTest()
    {
        return FNV1AYoshimitsuTRIAD32Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }

    [Benchmark]
    public unsafe uint MurmurHash32UnsafeTest()
    {
        return MurmurHash32Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }

    [Benchmark]
    public unsafe byte[] MurmurHash128UnsafeTest()
    {
        return MurmurHash128Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }

    [Benchmark]
    public unsafe ulong SipHash64UnsafeTest()
    {
        return SipHash64Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }
    [Benchmark]
    public unsafe uint SuperFastHash32UnsafeTest()
    {
        return SuperFastHash32Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }

    [Benchmark]
    public unsafe uint xxHash32UnsafeTest()
    {
        return xxHash32Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }

    [Benchmark]
    public unsafe ulong xxHash64UnsafeTest()
    {
        return xxHash64Unsafe.ComputeHash(_unsafePtr, _testData.Length);
    }

    private byte[] GetRandomBytes(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Number of bytes cannot be negative.");

        byte[] bytes = new byte[count];
        _rng.NextBytes(bytes);
        return bytes;
    }

    public void Dispose()
    {
        if (_ptr != IntPtr.Zero)
            Marshal.FreeHGlobal(_ptr);
    }
}