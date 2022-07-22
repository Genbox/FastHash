using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace Genbox.FastHash.Benchmarks;

[InProcess]
public class BigMulBenchmark
{
    [Benchmark]
    public Uint128 Dotnet_mul()
    {
        ulong high = Math.BigMul(10280214UL, 89244214UL, out ulong low);
        return new Uint128(low, high);
    }

    [Benchmark]
    public Uint128 xxHash_mul() => XxMul(10280214UL, 89244214UL);

    [Benchmark]
    public Uint128 Intrin() => Bmi2Mul(10280214UL, 89244214UL);

    [Benchmark]
    public Uint128 Scalar32() => Mul32(10280214UL, 89244214UL);

    [Benchmark]
    public Uint128 Scalar64() => Mul64(10280214UL, 89244214UL);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe Uint128 Bmi2Mul(ulong x, ulong y)
    {
        ulong low;
        ulong high = Bmi2.X64.MultiplyNoFlags(x, y, &low);
        return new Uint128(low, high);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Uint128 XxMul(ulong lhs, ulong rhs)
    {
        ulong lo_lo = XXH_mult32to64(lhs & 0xFFFFFFFF, rhs & 0xFFFFFFFF);
        ulong hi_lo = XXH_mult32to64(lhs >> 32, rhs & 0xFFFFFFFF);
        ulong lo_hi = XXH_mult32to64(lhs & 0xFFFFFFFF, rhs >> 32);
        ulong hi_hi = XXH_mult32to64(lhs >> 32, rhs >> 32);

        ulong cross = (lo_lo >> 32) + (hi_lo & 0xFFFFFFFF) + lo_hi;
        ulong upper = (hi_lo >> 32) + (cross >> 32) + hi_hi;
        ulong lower = (cross << 32) | (lo_lo & 0xFFFFFFFF);

        return new Uint128(lower, upper);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong XXH_mult32to64(ulong x, ulong y) => (uint)x * (ulong)(uint)y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Uint128 Mul32(ulong a, ulong b)
    {
        uint al = (uint)a;
        uint ah = (uint)(a >> 32);
        uint bl = (uint)b;
        uint bh = (uint)(b >> 32);

        ulong mull = (ulong)al * bl;
        ulong t = (ulong)ah * bl + (mull >> 32);
        ulong tl = (ulong)al * bh + (uint)t;

        ulong low = tl << 32 | (uint)mull;
        ulong high = (ulong)ah * bh + (t >> 32) + (tl >> 32);
        return new Uint128(low, high);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Uint128 Mul64(ulong x, ulong y)
    {
        ulong lo = x * y;

        ulong x0 = (uint)x;
        ulong x1 = x >> 32;

        ulong y0 = (uint)y;
        ulong y1 = y >> 32;

        ulong p11 = x1 * y1;
        ulong p01 = x0 * y1;
        ulong p10 = x1 * y0;
        ulong p00 = x0 * y0;

        // 64-bit product + two 32-bit values
        ulong middle = p10 + (p00 >> 32) + (uint)p01;

        // 64-bit product + two 32-bit values
        ulong hi = p11 + (middle >> 32) + (p01 >> 32);

        return new Uint128(lo, hi);
    }
}