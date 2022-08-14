using System.Runtime.CompilerServices;
using static Genbox.FastHash.FnvHash.FnvHashConstants;

namespace Genbox.FastHash.FnvHash;

/// <summary>Fowler–Noll–Vo hash implementation</summary>
public static class Fnv1aHash32
{
    public static uint ComputeHash(ReadOnlySpan<byte> data)
    {
        uint hash = FNV1_32_INIT;

        for (int i = 0; i < data.Length; i++)
            hash = (hash ^ data[i]) * FNV_32_PRIME;

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint hash = FNV1_32_INIT;
        hash = (hash ^ (input & 0xFF)) * FNV_32_PRIME;
        hash = (hash ^ ((input >> 8) & 0xFF)) * FNV_32_PRIME;
        hash = (hash ^ ((input >> 16) & 0xFF)) * FNV_32_PRIME;
        hash = (hash ^ ((input >> 24) & 0xFF)) * FNV_32_PRIME;
        return hash;
    }
}