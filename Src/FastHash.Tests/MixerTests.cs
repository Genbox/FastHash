using Genbox.FastHash.TestShared;
using static Genbox.FastHash.MixFunctions;

namespace Genbox.FastHash.Tests;

public class MixerTests
{
    [Theory]
    [MemberData(nameof(GetFunctions))]
    public void RandomDistributionTest(MixSpec64 spec)
    {
        int[] buckets = new int[100];
        const uint iterations = 1_000_000;

        for (ulong i = 0; i < iterations; i++)
        {
            ulong index = spec.Func(i) % (ulong)buckets.Length;
            buckets[index]++;
        }

        float onePercent = (float)iterations / buckets.Length;
        float fraction = onePercent * 0.04f; //4%

        //There should be 1% items in each bucket. We test if there are 4% of 1% deviation
        foreach (int val in buckets)
            Assert.True(Math.Abs(onePercent - val) < fraction);
    }

    public static TheoryData<MixSpec64> GetFunctions() => new TheoryData<MixSpec64>
    {
        new MixSpec64(AA_xmxmx_Murmur_64),
        new MixSpec64(JM_xmxmxmx_Mx3_64),
        new MixSpec64(PE_xmxmx_Moremur_64),
        new MixSpec64(YC_xmxmx_XXH2_64),
        new MixSpec64(EZ_xmx_FastHash_64),
        new MixSpec64(PE_rrxrrxmxxmxx_Nasam_64),
    };
}