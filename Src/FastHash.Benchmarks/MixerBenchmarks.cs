using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Order;
using static Genbox.FastHash.MixFunctions;

namespace Genbox.FastHash.Benchmarks;

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class MixerBenchmarks
{
    [Benchmark]
    [ArgumentsSource(nameof(GetMix64))]
    public ulong Mix64(MixSpec64 spec) => spec.Function(42);

    [Benchmark]
    [ArgumentsSource(nameof(GetMix32))]
    public ulong Mix32(MixSpec32 spec) => spec.Function(42);

    public static IEnumerable<object[]> GetMix64()
    {
        yield return [new MixSpec64(nameof(Murmur_64), Murmur_64)];
        yield return [new MixSpec64(nameof(I7_mrm_64), I7_mrm_64)];
        yield return [new MixSpec64(nameof(I8_mxm_64), I8_mxm_64)];
        yield return [new MixSpec64(nameof(I9_xmx_64), I9_xmx_64)];
        yield return [new MixSpec64(nameof(I11_mxma_64), I11_mxma_64)];
        yield return [new MixSpec64(nameof(I11_mxmx_64), I11_mxmx_64)];
        yield return [new MixSpec64(nameof(I12_xmrx_64), I12_xmrx_64)];
        yield return [new MixSpec64(nameof(I13_mxmxm_64), I13_mxmxm_64)];
        yield return [new MixSpec64(nameof(I14_mxrmx_64), I14_mxrmx_64)];
        yield return [new MixSpec64(nameof(I15_mxmxmx_64), I15_mxmxmx_64)];
        yield return [new MixSpec64(nameof(Mx2_64), Mx2_64)];
        yield return [new MixSpec64(nameof(Mx3_64), Mx3_64)];
        yield return [new MixSpec64(nameof(Rrmxmx), Rrmxmx)];
        yield return [new MixSpec64(nameof(MoreMur_64), MoreMur_64)];
        yield return [new MixSpec64(nameof(Nasam_64), Nasam_64)];
        yield return [new MixSpec64(nameof(Mix01), Mix01)];
        yield return [new MixSpec64(nameof(Mix02), Mix02)];
        yield return [new MixSpec64(nameof(Mix03), Mix03)];
        yield return [new MixSpec64(nameof(Mix04), Mix04)];
        yield return [new MixSpec64(nameof(Mix05), Mix05)];
        yield return [new MixSpec64(nameof(Mix06), Mix06)];
        yield return [new MixSpec64(nameof(Mix07), Mix07)];
        yield return [new MixSpec64(nameof(Mix08), Mix08)];
        yield return [new MixSpec64(nameof(Mix09), Mix09)];
        yield return [new MixSpec64(nameof(Mix10), Mix10)];
        yield return [new MixSpec64(nameof(Mix11), Mix11)];
        yield return [new MixSpec64(nameof(Mix12), Mix12)];
        yield return [new MixSpec64(nameof(Mix13), Mix13)];
        yield return [new MixSpec64(nameof(Mix14), Mix14)];
        yield return [new MixSpec64(nameof(FastHash_64), FastHash_64)];
        yield return [new MixSpec64(nameof(Lea_64), Lea_64)];
        yield return [new MixSpec64(nameof(Degski_64), Degski_64)];
        yield return [new MixSpec64(nameof(XXH2_64), XXH2_64)];
        yield return [new MixSpec64(nameof(XXH3_64), XXH3_64)];
        yield return [new MixSpec64(nameof(CityMix_64), CityMix_64)];
        yield return [new MixSpec64(nameof(Umash_64), Umash_64)];
        yield return [new MixSpec64(nameof(Wymix_64), Wymix_64)];
        yield return [new MixSpec64(nameof(Wymix2_64), Wymix2_64)];
        yield return [new MixSpec64(nameof(AxMix_64), AxMix_64)];
    }

    public static IEnumerable<object[]> GetMix32()
    {
        yield return [new MixSpec32(nameof(Murmur_32), Murmur_32)];
        yield return [new MixSpec32(nameof(Degski_32), Degski_32)];
        yield return [new MixSpec32(nameof(Fp64_32), Fp64_32)];
        yield return [new MixSpec32(nameof(XXH2_32), XXH2_32)];
        yield return [new MixSpec32(nameof(LowBias_32), LowBias_32)];
        yield return [new MixSpec32(nameof(Triple_32), Triple_32)];
    }

    [SuppressMessage("Design", "CA1034:Nested types should not be visible")]
    public readonly record struct MixSpec64(string Name, Func<ulong, ulong> Function)
    {
        public override string ToString() => Name;
    }

    [SuppressMessage("Design", "CA1034:Nested types should not be visible")]
    public readonly record struct MixSpec32(string Name, Func<uint, uint> Function)
    {
        public override string ToString() => Name;
    }
}