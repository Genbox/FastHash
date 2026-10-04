// C# implementation of FNV-1a by Glenn Fowler, Landon Curt Noll and Kiem-Phong Vo, which is in the public domain.
// See THIRD-PARTY-NOTICES.txt at the repository root.
using static Genbox.FastHash.FnvHash.FnvHashConstants;

namespace Genbox.FastHash.FnvHash;

/// <summary>Provides unsafe access to the 32-bit FNV-1a hash algorithm.</summary>
public static class Fnv1aHash32Unsafe
{
    /// <summary>Computes the hash of bytes at an unmanaged address.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> readable bytes; it may be null only when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 32-bit FNV-1a hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        uint hash = FNV1_32_INIT;

        for (int i = 0; i < length; i++)
        {
            hash ^= data[i];
            hash *= FNV_32_PRIME;
        }

        return hash;
    }
}