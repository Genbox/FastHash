// C# port of SuperFastHash by Paul Hsieh (http://www.azillionmonkeys.com/qed/hash.html).
// Distributed under the Paul Hsieh derivative license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;

namespace Genbox.FastHash.SuperFastHash;

/// <summary>Provides 32-bit SuperFastHash computations.</summary>
public static class SuperFastHash32
{
    /// <summary>Computes a hash index for <paramref name="input" /> using the default seed.</summary>
    /// <param name="input">The value to hash.</param>
    /// <returns>The 32-bit hash index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint hash = 4 + (uint)(ushort)input;
        hash = (hash << 16) ^ ((input >> 16) << 11) ^ hash;
        hash += hash >> 11;
        hash ^= hash << 3;
        hash += hash >> 5;
        hash ^= hash << 4;
        hash += hash >> 17;
        hash ^= hash << 25;
        hash += hash >> 6;
        return hash;
    }

    /// <summary>Computes a hash index for <paramref name="input" /> using <paramref name="seed" />.</summary>
    /// <param name="input">The value to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 32-bit hash index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input, uint seed)
    {
        uint hash = seed + (ushort)input;
        hash = (hash << 16) ^ ((input >> 16) << 11) ^ hash;
        hash += hash >> 11;
        hash ^= hash << 3;
        hash += hash >> 5;
        hash ^= hash << 4;
        hash += hash >> 17;
        hash ^= hash << 25;
        hash += hash >> 6;
        return hash;
    }

    /// <summary>Computes a 32-bit hash for <paramref name="data" /> using its length as the seed.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 32-bit hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data)
    {
        if (data.Length <= 0)
            return 0;

        return ComputeHash(data, (uint)data.Length);
    }

    /// <summary>Computes a 32-bit hash for <paramref name="data" /> using <paramref name="seed" />.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 32-bit hash.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data, uint seed)
    {
        if (data.Length <= 0)
            return 0;

        int length = data.Length;
        uint hash = seed, tmp;
        int rem = length & 3;
        length >>= 2;

        int index = 0;

        for (; length > 0; length--)
        {
            hash += Read16(data, index);
            tmp = (uint)((Read16(data, index + 2) << 11) ^ hash);
            hash = (hash << 16) ^ tmp;
            index += 2 * sizeof(ushort);
            hash += hash >> 11;
        }

        switch (rem)
        {
            case 3:
                hash += Read16(data, index);
                hash ^= hash << 16;
                hash ^= (uint)(data[index + sizeof(ushort)] << 18);
                hash += hash >> 11;
                break;
            case 2:
                hash += Read16(data, index);
                hash ^= hash << 11;
                hash += hash >> 17;
                break;
            case 1:
                hash += data[index];
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