using BenchmarkDotNet.Order;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.GxHash;
using Genbox.FastHash.MarvinHash;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Benchmarks;

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Index32Benchmarks
{
    private readonly uint _value = 1337; // Keep readonly and non-static. Otherwise, compiler will inline and skew results.

    [Benchmark]
    public uint CityHash32Test() => CityHash32.ComputeIndex(_value);

    [Benchmark]
    public uint Djb2AltHash32Test() => Djb2AltHash32.ComputeIndex(_value);

    [Benchmark]
    public uint Djb2Hash32Test() => Djb2Hash32.ComputeIndex(_value);

    [Benchmark]
    public uint FarmHash32Test() => FarmHash32.ComputeIndex(_value);

    [Benchmark]
    public uint Fnv1aHash32Test() => Fnv1aHash32.ComputeIndex(_value);

    [Benchmark]
    public uint Gx2Hash32Test() => Gx2Hash32.ComputeIndex(_value);

    [Benchmark]
    public uint MarvinHash32Test() => MarvinHash32.ComputeIndex(_value);

    [Benchmark]
    public uint Murmur3Hash32Test() => Murmur3Hash32.ComputeIndex(_value);

    [Benchmark]
    public uint SuperFastHash32Test() => SuperFastHash32.ComputeIndex(_value);

    [Benchmark]
    public uint XxHash32Test() => XxHash32.ComputeIndex(_value);
}