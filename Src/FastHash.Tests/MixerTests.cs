using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static Genbox.FastHash.MixFunctions;

namespace Genbox.FastHash.Tests;

[SuppressMessage("Design", "CA1034:Nested types should not be visible")]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal")]
public class MixerTests
{
    [Theory]
    [MemberData(nameof(GetFunctions))]
    public void RandomDistributionTest(MixSpec spec)
    {
        int[] buckets = new int[100];
        uint iterations = 1_000_000;

        for (ulong i = 0; i < iterations; i++)
        {
            ulong index = spec.Func(i) % (ulong)buckets.Length;
            buckets[index]++;
        }

        float onePercent = (float)iterations / buckets.Length;
        float fraction = onePercent * 0.04f; //4%

        //There should be 1% items in each bucket. We test if there are 4% of 1% deviation
        for (int i = 0; i < buckets.Length; i++)
            Assert.True(Math.Abs(onePercent - buckets[i]) < fraction);
    }

    public static IEnumerable<object[]> GetFunctions()
    {
        yield return [new MixSpec(AA_xmxmx_Murmur_64)];
        yield return [new MixSpec(JM_xmxmxmx_Mx3_64)];
        yield return [new MixSpec(PE_xmxmx_Moremur_64)];
        yield return [new MixSpec(YC_xmxmx_XXH2_64)];
        yield return [new MixSpec(EZ_xmx_FastHash_64)];
        yield return [new MixSpec(PE_rrxrrxmxxmxx_Nasam_64)];
    }

    public record MixSpec(Func<ulong, ulong> Func, [CallerArgumentExpression(nameof(Func))]string Name = "")
    {
        public override string ToString() => Name;
    }
}