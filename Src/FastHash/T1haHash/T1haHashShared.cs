using System.Runtime.CompilerServices;
using static Genbox.FastHash.T1haHash.T1haHashConstants;

namespace Genbox.FastHash.T1haHash;

internal static class T1haHashShared
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void MixUp(ref ulong x, ref ulong y, ulong value, ulong prime)
    {
        ulong high = BigMul(y + value, prime, out ulong low);
        x ^= low;
        y += high;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Final64(ulong a, ulong b)
    {
        ulong x = (a + RotateRight(b, 41)) * Prime0;
        ulong y = (RotateRight(a, 23) + b) * Prime6;
        return Fold128To64(x ^ y, Prime5);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Tail64(ReadOnlySpan<byte> data, int length)
    {
        ulong value = 0;

        for (int i = 0; i < length; i++)
            value |= (ulong)data[i] << (8 * i);

        return value;
    }
}