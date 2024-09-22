using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Genbox.FastHash.TestShared;

public static class Mult
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong, ulong) MathBigMul(ulong a, ulong b)
    {
        ulong high = Math.BigMul(a, b, out ulong low);
        return (low, high);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong, ulong) XxHashMul(ulong a, ulong b)
    {
        unchecked
        {
            ulong y = b & 0xFFFFFFFF;
            ulong lo_lo = (uint)(a & 0xFFFFFFFF) * (ulong)(uint)y;
            ulong y1 = b & 0xFFFFFFFF;
            ulong hi_lo = (uint)(a >> 32) * (ulong)(uint)y1;
            ulong y2 = b >> 32;
            ulong lo_hi = (uint)(a & 0xFFFFFFFF) * (ulong)(uint)y2;
            ulong y3 = b >> 32;
            ulong hi_hi = (uint)(a >> 32) * (ulong)(uint)y3;

            ulong cross = (lo_lo >> 32) + (hi_lo & 0xFFFFFFFF) + lo_hi;
            ulong upper = (hi_lo >> 32) + (cross >> 32) + hi_hi;
            ulong lower = (cross << 32) | (lo_lo & 0xFFFFFFFF);
            return (lower, upper);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe (ulong, ulong) Bmi2Mul(ulong a, ulong b)
    {
        ulong low;
        ulong high = Bmi2.X64.MultiplyNoFlags(a, b, &low);
        return (low, high);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong, ulong) Scalar32Mul(ulong a, ulong b)
    {
        unchecked
        {
            uint al = (uint)a;
            uint ah = (uint)(a >> 32);
            uint bl = (uint)b;
            uint bh = (uint)(b >> 32);

            ulong mull = (ulong)al * bl;
            ulong t = ((ulong)ah * bl) + (mull >> 32);
            ulong tl = ((ulong)al * bh) + (uint)t;

            ulong low = (tl << 32) | (uint)mull;
            ulong high = ((ulong)ah * bh) + (t >> 32) + (tl >> 32);
            return (low, high);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong, ulong) Scalar64Mul(ulong a, ulong b)
    {
        unchecked
        {
            ulong lo = a * b;

            ulong x0 = (uint)a;
            ulong x1 = a >> 32;

            ulong y0 = (uint)b;
            ulong y1 = b >> 32;

            ulong p11 = x1 * y1;
            ulong p01 = x0 * y1;
            ulong p10 = x1 * y0;
            ulong p00 = x0 * y0;

            ulong middle = p10 + (p00 >> 32) + (uint)p01;
            ulong hi = p11 + (middle >> 32) + (p01 >> 32);

            return (lo, hi);
        }
    }
}