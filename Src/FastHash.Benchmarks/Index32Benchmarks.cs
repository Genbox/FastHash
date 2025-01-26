using BenchmarkDotNet.Order;
using Genbox.FastHash.Benchmarks.Code;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.MarvinHash;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Benchmarks;

[MbPrSecColumn(4)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Index32Benchmarks
{
    private readonly uint _value = 12808241;

    [Benchmark]
    public uint CityHash32Test() => CityHash32.ComputeIndex(_value);

    [Benchmark]
    public uint Djb2Hash32Test() => Djb2Hash32.ComputeIndex(_value);

    [Benchmark]
    public uint FarmHash32Test() => FarmHash32.ComputeIndex(_value);

    [Benchmark]
    public uint Fnv1aHash32Test() => Fnv1aHash32.ComputeIndex(_value);

    [Benchmark]
    public uint MarvinHash32Test() => MarvinHash32.ComputeIndex(_value);

    [Benchmark]
    public uint Murmur3Hash32Test() => Murmur3Hash32.ComputeIndex(_value);

    [Benchmark]
    public uint SuperFastHash32Test() => SuperFastHash32.ComputeIndex(_value);

    [Benchmark]
    public uint Xx2Hash32Test() => Xx2Hash32.ComputeIndex(_value);
}