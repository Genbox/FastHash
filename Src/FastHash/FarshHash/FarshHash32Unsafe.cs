// C# port of Bulat-Ziganshin/FARSH 0.2.0 (d74ef3a). Copyright (c) 2015-16 Bulat Ziganshin.
// Distributed under the MIT license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using static Genbox.FastHash.FarshHash.FarshHashConstants;

namespace Genbox.FastHash.FarshHash;

/// <summary>Provides unsafe access to the 32-bit FARSH algorithm.</summary>
public static class FarshHash32Unsafe
{
    /// <summary>Computes the hash of bytes at an unmanaged address using a zero seed.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> readable bytes; it may be null only when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 32-bit FARSH value.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return ComputeHash(data, length, 0);
    }

    /// <summary>Computes the hash of bytes at an unmanaged address.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> readable bytes; it may be null only when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 32-bit FARSH value.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe uint ComputeHash(byte* data, int length, ulong seed)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        ulong sum = seed;

        while (length >= STRIPE)
        {
            sum = farsh_combine(sum, farsh_full_block(data));
            data += STRIPE;
            length -= STRIPE;
        }

        if (length > 0)
            sum = farsh_combine(sum, farsh_partial_block(data, length));

        return farsh_final(sum) ^ FARSH_KEYS[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong farsh_full_block(byte* data)
    {
        ulong sum = 0;

        for (int i = 0; i < STRIPE_ELEMENTS; i += 2)
        {
            uint val1 = Read32(data + (i * sizeof(uint)));
            uint val2 = Read32(data + ((i + 1) * sizeof(uint)));
            sum += (val1 + FARSH_KEYS[i]) * (ulong)(val2 + FARSH_KEYS[i + 1]);
        }

        return sum;
    }

    private static unsafe ulong farsh_partial_block(byte* data, int length)
    {
        ulong sum = 0;
        int elements = (length / sizeof(uint)) & ~1;
        int i;

        for (i = 0; i < elements; i += 2)
        {
            uint val1 = Read32(data + (i * sizeof(uint)));
            uint val2 = Read32(data + ((i + 1) * sizeof(uint)));
            sum += (val1 + FARSH_KEYS[i]) * (ulong)(val2 + FARSH_KEYS[i + 1]);
            length -= 8;
        }

        data += elements * sizeof(uint);

        uint v1;
        uint v2;

        byte* ptr = data;

        switch (length)
        {
            case 7:
                v1 = Read32(ptr);
                ptr += 4;
                v2 = (uint)(ptr[0] | (ptr[1] << 8) | (ptr[2] << 16));
                AddPartial(v1, v2);
                break;
            case 6:
                v1 = Read32(ptr);
                ptr += 4;
                v2 = Read16(ptr);
                AddPartial(v1, v2);
                break;
            case 5:
                v1 = Read32(ptr);
                ptr += 4;
                v2 = *ptr;
                AddPartial(v1, v2);
                break;
            case 4:
                v1 = Read32(ptr);
                AddPartial(v1, 0);
                break;
            case 3:
                v1 = (uint)(ptr[0] | (ptr[1] << 8) | (ptr[2] << 16));
                AddPartial(v1, 0);
                break;
            case 2:
                v1 = Read16(ptr);
                AddPartial(v1, 0);
                break;
            case 1:
                v1 = *ptr;
                AddPartial(v1, 0);
                break;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddPartial(uint value1, uint value2) => sum += (value1 + FARSH_KEYS[i]) * (ulong)(value2 + FARSH_KEYS[i + 1]);

        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong farsh_combine(ulong sum, ulong h)
    {
        h *= PRIME64_2;
        h += h >> 31;
        h *= PRIME64_1;
        sum ^= h;
        sum = ((sum + (sum >> 27)) * PRIME64_1) + PRIME64_4;
        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint farsh_final(ulong sum)
    {
        sum ^= sum >> 33;
        sum *= PRIME64_2;
        sum ^= sum >> 29;
        sum *= PRIME64_3;
        return (uint)sum ^ (uint)(sum >> 32);
    }
}