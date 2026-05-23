using BenchmarkDotNet.Order;
using Genbox.FastHash.AesniHash;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.GxHash;
using Genbox.FastHash.MeowHash;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Benchmarks;

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Index128Benchmarks
{
    private readonly ulong _value = 1337; // Keep readonly and non-static. Otherwise, compiler will inline and skew results.

    [Benchmark]
    public UInt128 AesniHash128Test() => AesniHash128.ComputeIndex(_value);

    [Benchmark]
    public UInt128 CityHash128Test() => CityHash128.ComputeIndex(_value);

    [Benchmark]
    public UInt128 FarmHash128Test() => FarmHash128.ComputeIndex(_value);

    [Benchmark]
    public UInt128 Gx2Hash128Test() => Gx2Hash128.ComputeIndex(_value);

    [Benchmark]
    public UInt128 MeowHash128UnsafeTest() => MeowHash128Unsafe.ComputeIndex(_value);

    [Benchmark]
    public UInt128 Murmur3Hash128Test() => Murmur3Hash128.ComputeIndex(_value);

    [Benchmark]
    public UInt128 Xx3Hash128Test() => Xx3Hash128.ComputeIndex(_value);
}