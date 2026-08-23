using static Genbox.FastHash.MurmurHash.MurmurHashConstants;

namespace Genbox.FastHash.MurmurHash;

/// <summary>Provides pointer-based access to the 32-bit MurmurHash3 algorithm.</summary>
public static class Murmur3Hash32Unsafe
{
    /// <summary>Computes the hash of an unmanaged byte sequence using a zero seed.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> bytes, or any pointer when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 32-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return ComputeHash(data, length, 0);
    }

    /// <summary>Computes the hash of an unmanaged byte sequence.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> bytes.</param><param name="length">The number of bytes to hash.</param><param name="seed">The hash seed.</param><returns>The 32-bit hash.</returns><exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe uint ComputeHash(byte* data, int length, uint seed)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        int nblocks = length / 4;
        uint h1 = seed;
        uint k1;

        for (int i = 0; i < nblocks; i++)
        {
            k1 = Read32(data + (i * 4));

            k1 *= C1_32;
            k1 = RotateLeft(k1, 15);
            k1 *= C2_32;

            h1 ^= k1;
            h1 = RotateLeft(h1, 13);
            h1 = (h1 * 5) + 0xe6546b64;
        }

        byte* tail = data + (nblocks * 4);
        k1 = 0;

        switch (length & 3)
        {
            case 3:
                k1 ^= (uint)tail[2] << 16;
                goto case 2;
            case 2:
                k1 ^= (uint)tail[1] << 8;
                goto case 1;
            case 1:
                k1 ^= tail[0];
                break;
        }

        k1 *= C1_32;
        k1 = RotateLeft(k1, 15);
        k1 *= C2_32;
        h1 ^= k1;

        uint len = (uint)length;

        h1 ^= len;
        h1 = AA_xmxmx_Murmur_32(h1);

        return h1;
    }
}