using System.Runtime.CompilerServices;
using static Genbox.FastHash.FnvHash.FnvHashConstants;

namespace Genbox.FastHash.FnvHash;

/// <summary>Provides the 64-bit FNV-1a hash algorithm.</summary>
public static class Fnv1aHash64
{
    /// <summary>Computes the hash of a 64-bit integer.</summary>
    /// <param name="input">The integer to hash.</param>
    /// <returns>The 64-bit FNV-1a hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        ulong hash = FNV1_64_INIT;
        hash = (hash ^ (input & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 8) & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 16) & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 24) & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 32) & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 40) & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 48) & 0xFF)) * FNV_64_PRIME;
        hash = (hash ^ ((input >> 56) & 0xFF)) * FNV_64_PRIME;
        return hash;
    }

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 64-bit FNV-1a hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data)
    {
        ulong hash = FNV1_64_INIT;

        for (int i = 0; i < data.Length; i++)
        {
            hash ^= data[i];
            hash *= FNV_64_PRIME;
        }

        return hash;
    }
}