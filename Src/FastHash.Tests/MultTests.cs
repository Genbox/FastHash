using Genbox.FastHash.TestShared;
using Genbox.FastHash.Misc;
using System.Runtime.Intrinsics.X86;

namespace Genbox.FastHash.Tests;

public class MultTests
{
    private readonly (ulong, ulong) _result = (917449618181796, 0);
    private readonly ulong _valA = 10280214UL;
    private readonly ulong _valB = 89244214UL;

    [Fact]
    public void MathBigMul() => Assert.Equal(_result, Mult.MathBigMul(_valA, _valB));

    [Fact]
    public void XxHashMul() => Assert.Equal(_result, Mult.XxHashMul(_valA, _valB));

    [Fact]
    public void Bmi2Mul() => Assert.Equal(_result, Mult.Bmi2Mul(_valA, _valB));

    [Fact]
    public void Scalar32Mul() => Assert.Equal(_result, Mult.Scalar32Mul(_valA, _valB));

    [Fact]
    public void Scalar64Mul() => Assert.Equal(_result, Mult.Scalar64Mul(_valA, _valB));

    [Fact]
    public void Scalar64ProductionMulMatchesMathBigMulAtBoundaries()
    {
        ulong[] values = [0, 1, 2, uint.MaxValue, (ulong)uint.MaxValue + 1, ulong.MaxValue - 1, ulong.MaxValue];

        foreach (ulong a in values)
        {
            foreach (ulong b in values)
                AssertBigMul(a, b);
        }
    }

    [Fact]
    public void Scalar64ProductionMulMatchesMathBigMulForRandomInputs()
    {
        Random random = new(0x51A1A64);

        for (int i = 0; i < 10_000; i++)
            AssertBigMul(NextUInt64(random), NextUInt64(random));
    }

    private static void AssertBigMul(ulong a, ulong b)
    {
        ulong expectedHigh = Math.BigMul(a, b, out ulong expectedLow);
        ulong actualHigh = Utilities.BigMulScalar(a, b, out ulong actualLow);

        Assert.Equal(expectedLow, actualLow);
        Assert.Equal(expectedHigh, actualHigh);
    }

    private static ulong NextUInt64(Random random) => ((ulong)random.NextInt64() << 1) | (uint)random.Next(2);
}