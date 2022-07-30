using System.Runtime.CompilerServices;

namespace Genbox.FastHash.MurmurHash;

internal static class MurmurShared
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint MurmurMix(uint h)
    {
        h ^= h >> 16;
        h *= 0x85ebca6b;
        h ^= h >> 13;
        h *= 0xc2b2ae35;
        h ^= h >> 16;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong MurmurMix(ulong h)
    {
        h ^= h >> 33;
        h *= 0xff51afd7ed558ccd;
        h ^= h >> 33;
        h *= 0xc4ceb9fe1a85ec53;
        h ^= h >> 33;
        return h;
    }
}