using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Order;
using static Genbox.FastHash.MixFunctions;

namespace Genbox.FastHash.Benchmarks;

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal")]
[SuppressMessage("Design", "CA1034:Nested types should not be visible")]
public class MixerBenchmarks
{
    [Benchmark]
    [ArgumentsSource(nameof(GetMix64))]
    public ulong Mix64(MixSpec64 spec) => spec.Func(42);

    [Benchmark]
    [ArgumentsSource(nameof(GetMix32))]
    public ulong Mix32(MixSpec32 spec) => spec.Func(42);

    public static IEnumerable<object[]> GetMix64()
    {
        yield return [new MixSpec64(AA_xmxmx_Murmur_64)];
        yield return [new MixSpec64(JM_mrm_Depth7_64)];
        yield return [new MixSpec64(JM_mxm_Depth8_64)];
        yield return [new MixSpec64(JM_xmx_Depth9_64)];
        yield return [new MixSpec64(JM_mxma_Depth11_64)];
        yield return [new MixSpec64(JM_mxmx_Depth11_64)];
        yield return [new MixSpec64(JM_xmrx_Depth12_64)];
        yield return [new MixSpec64(JM_mxmxm_Depth13_64)];
        yield return [new MixSpec64(JM_mxrmx_Depth14_64)];
        yield return [new MixSpec64(JM_mxmxmx_Depth15_64)];
        yield return [new MixSpec64(JM_xmxmx_Mx2_64)];
        yield return [new MixSpec64(JM_xmxmxmx_Mx3_64)];
        yield return [new MixSpec64(PE_rrxrrxmxmx_rrmxmx_64)];
        yield return [new MixSpec64(PE_rrxrrmrrxrrxmx_rrxmrrxmsx0_64)];
        yield return [new MixSpec64(PE_xmxmx_Moremur_64)];
        yield return [new MixSpec64(PE_rrxrrxmxxmxx_Nasam_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix01_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix02_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix03_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix04_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix05_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix06_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix07_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix08_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix09_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix10_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix11_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix12_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix13_64)];
        yield return [new MixSpec64(DS_xmxmx_Mix14_64)];
        yield return [new MixSpec64(EZ_xmx_FastHash_64)];
        yield return [new MixSpec64(DL_xmxmx_Lea_64)];
        yield return [new MixSpec64(DE_xmxmx_Degski_64)];
        yield return [new MixSpec64(YC_xmxmx_XXH2_64)];
        yield return [new MixSpec64(YC_xmx_XXH3_64)];
        yield return [new MixSpec64(GP_mxxmxxm_CityHash_64)];
        yield return [new MixSpec64(PK_rlxrlx_Umash_64)];
        yield return [new MixSpec64(WF_amx_Wymix_64)];
        yield return [new MixSpec64(WF_amxmx_Wymix_64)];
        yield return [new MixSpec64(TE_rlxrlmrlxrlx_AxMix_64)];
    }

    public static IEnumerable<object[]> GetMix32()
    {
        yield return [new MixSpec32(AA_xmxmx_Murmur_32)];
        yield return [new MixSpec32(DE_xmxmx_Degski_32)];
        yield return [new MixSpec32(FP_xsxxmx_Fp64_32)];
        yield return [new MixSpec32(YC_xmxmx_XXH2_32)];
        yield return [new MixSpec32(CW_xmxmx_LowBias_32)];
        yield return [new MixSpec32(CW_xmxmxmx_Triple_32)];
    }

    public record MixSpec64(Func<ulong, ulong> Func, [CallerArgumentExpression(nameof(Func))]string Name = "")
    {
        public override string ToString() => Name;
    }

    public record MixSpec32(Func<uint, uint> Func, [CallerArgumentExpression(nameof(Func))]string Name = "")
    {
        public override string ToString() => Name;
    }
}