using System.Runtime.CompilerServices;

namespace Genbox.FastHash.RapidHash;

internal static class RapidHashShared
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong RapidMix(ulong a, ulong b)
    {
        ulong high = BigMul(a, b, out ulong low);
        return low ^ high;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void RapidMum(ref ulong a, ref ulong b)
    {
        ulong high = BigMul(a, b, out ulong low);
        a = low;
        b = high;
    }
}
