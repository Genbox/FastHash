using System.Runtime.CompilerServices;

namespace Genbox.FastHash;

internal static class Shared
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static UInt128 Multiply64To128(ulong a, ulong b)
    {
        ulong high = BigMul(a, b, out ulong low);
        return new UInt128(low, high);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Fold128To64(ulong a, ulong b)
    {
        ulong high = BigMul(a, b, out ulong low);
        return low ^ high;
    }
}