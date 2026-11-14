// C# port of wangyi-fudan/wyhash final version 2 (59aacba) by Wang Yi.
// Distributed under the Unlicense; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;

namespace Genbox.FastHash.WyHash;

internal static class WyHashShared
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Wymum(ref ulong a, ref ulong b)
    {
        ulong high = BigMul(a, b, out ulong low);
#if WYHASH_CONDOM
        a ^= low;
        b ^= high;
#else
        a = low;
        b = high;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Wymix(ulong a, ulong b)
    {
        Wymum(ref a, ref b);
        return a ^ b;
    }
}