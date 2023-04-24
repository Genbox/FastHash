using Genbox.FastHash.TestShared;

namespace Genbox.FastHash.Tests;

public class MultTests
{
    private readonly ulong _valA = 10280214UL;
    private readonly ulong _valB = 89244214UL;
    private readonly (ulong, ulong) _result = (917449618181796, 0);

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
}