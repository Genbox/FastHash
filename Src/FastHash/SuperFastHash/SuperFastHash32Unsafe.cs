namespace Genbox.FastHash.SuperFastHash;

/// <summary>Provides unsafe 32-bit SuperFastHash computations over unmanaged memory.</summary>
public static class SuperFastHash32Unsafe
{
    /// <summary>Computes a 32-bit hash for an unmanaged byte sequence using its length as the seed.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> bytes, or null when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 32-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (data == null || length == 0)
            return 0;

        return ComputeHash(data, length, (uint)length);
    }

    /// <summary>Computes a 32-bit hash for an unmanaged byte sequence using <paramref name="seed" />.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> bytes, or null when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 32-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe uint ComputeHash(byte* data, int length, uint seed)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (data == null || length == 0)
            return 0;

        uint hash = seed;
        int rem = length & 3;
        length >>= 2;

        for (; length > 0; length--)
        {
            hash += Read16(data);
            hash = (hash << 16) ^ (uint)((Read16(data + 2) << 11) ^ hash);
            data += 2 * sizeof(ushort);
            hash += hash >> 11;
        }

        switch (rem)
        {
            case 3:
                hash += Read16(data);
                hash ^= hash << 16;
                hash ^= (uint)(data[sizeof(ushort)] << 18);
                hash += hash >> 11;
                break;
            case 2:
                hash += Read16(data);
                hash ^= hash << 11;
                hash += hash >> 17;
                break;
            case 1:
                hash += *data;
                hash ^= hash << 10;
                hash += hash >> 1;
                break;
        }

        // Force "avalanching" of final 127 bits
        hash ^= hash << 3;
        hash += hash >> 5;
        hash ^= hash << 4;
        hash += hash >> 17;
        hash ^= hash << 25;
        hash += hash >> 6;

        return hash;
    }
}