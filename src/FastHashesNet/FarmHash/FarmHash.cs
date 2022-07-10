using System.Runtime.CompilerServices;

namespace Genbox.FastHashesNet.FarmHash;

internal static class FarmHash
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Mur(uint a, uint h)
    {
        // Helper from Murmur3 for combining two 32-bit values.
        a *= FarmHashConstants.c1;
        a = Utilities.RotateRightCheck(a, 17);
        a *= FarmHashConstants.c2;
        h ^= a;
        h = Utilities.RotateRightCheck(h, 19);
        return h * 5 + 0xe6546b64;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong ShiftMix(ulong val) => val ^ (val >> 47);
}