using static Genbox.FastHash.FnvHash.FnvHashConstants;

namespace Genbox.FastHash.FnvHash;

/// <summary>Provides unsafe access to the 64-bit FNV-1a hash algorithm.</summary>
public static class Fnv1aHash64Unsafe
{
    /// <summary>Computes the hash of bytes at an unmanaged address.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> readable bytes; it may be null only when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 64-bit FNV-1a hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        ulong hash = FNV1_64_INIT;

        for (int i = 0; i < length; i++)
        {
            hash ^= data[i];
            hash *= FNV_64_PRIME;
        }

        return hash;
    }
}