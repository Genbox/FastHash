using static Genbox.FastHash.DjbHash.DjbHashConstants;

namespace Genbox.FastHash.DjbHash;

/// <summary>Provides unsafe access to the 64-bit XOR variant of the DJB2 hash algorithm.</summary>
public static class Djb2AltHash64Unsafe
{
    /// <summary>Computes the hash of bytes at an unmanaged address.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> readable bytes; it may be null only when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 64-bit DJB2 XOR hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        ulong hash = InitHash;

        for (int i = 0; i < length; i++)
            hash = ((hash << 5) + hash) ^ data[i];

        return hash;
    }
}