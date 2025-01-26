using Genbox.FastHash.TestShared;

namespace Genbox.FastHash.Benchmarks;

public class Multiply64Benchmarks
{
    private readonly ulong _valA = 10280214UL;
    private readonly ulong _valB = 89244214UL;

    [Benchmark]
    public (ulong, ulong) MathBigMul() => Mult.MathBigMul(_valA, _valB);

    [Benchmark]
    public (ulong, ulong) XxHashMul() => Mult.XxHashMul(_valA, _valB);

    [Benchmark]
    public (ulong, ulong) Bmi2Mul() => Mult.Bmi2Mul(_valA, _valB);

    [Benchmark]
    public (ulong, ulong) Scalar32Mul() => Mult.Scalar32Mul(_valA, _valB);

    [Benchmark]
    public (ulong, ulong) Scalar64Mul() => Mult.Scalar64Mul(_valA, _valB);
}