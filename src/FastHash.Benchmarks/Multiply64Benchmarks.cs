using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace Genbox.FastHash.Benchmarks;

[InProcess]
public class Multiply64Benchmarks
{
    private ulong _valA = 10280214UL;
    private ulong _valB = 89244214UL;

    [Benchmark]
    public Uint128 MathBigMul()
    {
        ulong high = Math.BigMul(_valA, _valB, out ulong low);
        return new Uint128(low, high);
    }

    [Benchmark]
    public Uint128 XxHashMul()
    {
        ulong y = _valB & 0xFFFFFFFF;
        ulong lo_lo = (uint)(_valA & 0xFFFFFFFF) * (ulong)(uint)y;
        ulong y1 = _valB & 0xFFFFFFFF;
        ulong hi_lo = (uint)(_valA >> 32) * (ulong)(uint)y1;
        ulong y2 = _valB >> 32;
        ulong lo_hi = (uint)(_valA & 0xFFFFFFFF) * (ulong)(uint)y2;
        ulong y3 = _valB >> 32;
        ulong hi_hi = (uint)(_valA >> 32) * (ulong)(uint)y3;

        ulong cross = (lo_lo >> 32) + (hi_lo & 0xFFFFFFFF) + lo_hi;
        ulong upper = (hi_lo >> 32) + (cross >> 32) + hi_hi;
        ulong lower = (cross << 32) | (lo_lo & 0xFFFFFFFF);
        return new Uint128(lower, upper);
    }

    [Benchmark]
    public unsafe Uint128 Bmi2Mul()
    {
        ulong low;
        ulong high = Bmi2.X64.MultiplyNoFlags(_valA, _valB, &low);
        return new Uint128(low, high);
    }

    [Benchmark]
    public Uint128 Scalar32Mul()
    {
        uint al = (uint)_valA;
        uint ah = (uint)(_valA >> 32);
        uint bl = (uint)_valB;
        uint bh = (uint)(_valB >> 32);

        ulong mull = (ulong)al * bl;
        ulong t = (ulong)ah * bl + (mull >> 32);
        ulong tl = (ulong)al * bh + (uint)t;

        ulong low = tl << 32 | (uint)mull;
        ulong high = (ulong)ah * bh + (t >> 32) + (tl >> 32);
        return new Uint128(low, high);
    }

    [Benchmark]
    public Uint128 Scalar64Mul()
    {
        ulong lo = _valA * _valB;

        ulong x0 = (uint)_valA;
        ulong x1 = _valA >> 32;

        ulong y0 = (uint)_valB;
        ulong y1 = _valB >> 32;

        ulong p11 = x1 * y1;
        ulong p01 = x0 * y1;
        ulong p10 = x1 * y0;
        ulong p00 = x0 * y0;

        ulong middle = p10 + (p00 >> 32) + (uint)p01;
        ulong hi = p11 + (middle >> 32) + (p01 >> 32);

        return new Uint128(lo, hi);
    }
}