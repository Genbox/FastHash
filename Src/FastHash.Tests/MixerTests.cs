using System.Diagnostics.CodeAnalysis;
using static Genbox.FastHash.MixFunctions;

namespace Genbox.FastHash.Tests;

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
        yield return [new MixSpec(nameof(Murmur_64), Murmur_64)];
        yield return [new MixSpec(nameof(Mx3_64), Mx3_64)];
        yield return [new MixSpec(nameof(MoreMur_64), MoreMur_64)];
        yield return [new MixSpec(nameof(XXH2_64), XXH2_64)];
        yield return [new MixSpec(nameof(FastHash_64), FastHash_64)];
        yield return [new MixSpec(nameof(Nasam_64), Nasam_64)];
    }

    [SuppressMessage("Design", "CA1034:Nested types should not be visible")]
    public readonly record struct MixSpec(string Name, Func<ulong, ulong> Function)
    {
        public override string ToString() => Name;
    }
}