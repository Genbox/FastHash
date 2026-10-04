// C# implementation of FNV-1a by Glenn Fowler, Landon Curt Noll and Kiem-Phong Vo, which is in the public domain.
// See THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using static Genbox.FastHash.FnvHash.FnvHashConstants;

namespace Genbox.FastHash.FnvHash;

/// <summary>Provides the 32-bit FNV-1a hash algorithm.</summary>
public static class Fnv1aHash32
{
    /// <summary>Computes the hash of a 32-bit integer.</summary>
    /// <param name="input">The integer to hash.</param>
    /// <returns>The 32-bit FNV-1a hash.</returns>
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

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 32-bit FNV-1a hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data)
    {
        uint hash = FNV1_32_INIT;

        for (int i = 0; i < data.Length; i++)
            hash = (hash ^ data[i]) * FNV_32_PRIME;

        return hash;
    }
}