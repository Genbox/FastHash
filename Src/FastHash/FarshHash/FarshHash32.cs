using System.Runtime.CompilerServices;
using static Genbox.FastHash.FarshHash.FarshHashConstants;

namespace Genbox.FastHash.FarshHash;

/// <summary>Provides the 32-bit FARSH algorithm.</summary>
public static class FarshHash32
{
    /// <summary>Computes the hash of a 32-bit integer using a zero seed.</summary>
    /// <param name="input">The integer to hash.</param>
    /// <returns>The 32-bit FARSH value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input) => ComputeIndex(input, 0);

    /// <summary>Computes the hash of a 32-bit integer.</summary>
    /// <param name="input">The integer to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 32-bit FARSH value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input, ulong seed)
    {
        ulong hash = (input + FARSH_KEYS[0]) * (ulong)FARSH_KEYS[1];
        return farsh_final(farsh_combine(seed, hash)) ^ FARSH_KEYS[0];
    }

    /// <summary>Computes the hash of a byte sequence using a zero seed.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 32-bit FARSH value.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 32-bit FARSH value.</returns>
    public static uint ComputeHash(ReadOnlySpan<byte> data, ulong seed)
    {
        ulong sum = seed;
        uint length = (uint)data.Length;
        int offset = 0;

        while (length >= STRIPE)
        {
            sum = farsh_combine(sum, farsh_full_block(data, offset));
            offset += STRIPE;
            length -= STRIPE;
        }

        if (length > 0)
            sum = farsh_combine(sum, farsh_partial_block(data, offset));

        return farsh_final(sum) ^ FARSH_KEYS[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong farsh_full_block(ReadOnlySpan<byte> data, int offset)
    {
        ulong sum = 0;

        uint j = 0;

        for (uint i = 0; i < STRIPE; i += 8, j += 2)
        {
            uint val1 = Read32(data, (uint)offset + i);
            uint val2 = Read32(data, (uint)offset + i + sizeof(uint));
            sum += (val1 + FARSH_KEYS[j]) * (ulong)(val2 + FARSH_KEYS[j + 1]);
        }

        return sum;
    }

    private static ulong farsh_partial_block(ReadOnlySpan<byte> data, int offset)
    {
        ulong sum = 0;
        int keyindex = 0;
        int length = data.Length;

        uint chunks = (uint)((length - offset) >> 3);

        for (; chunks > 0; chunks--)
        {
            uint val1 = Read32(data, offset);
            uint val2 = Read32(data, offset + sizeof(uint));
            sum += (val1 + FARSH_KEYS[keyindex]) * (ulong)(val2 + FARSH_KEYS[keyindex + 1]);
            offset += 8;
            keyindex += 2;
        }

        uint v1;
        uint v2;
        uint remaining = (uint)(length - offset);

        switch (remaining)
        {
            case 7:
                v1 = Read32(data, offset);
                offset += 4;
                v2 = (uint)(data[0 + offset] | (data[1 + offset] << 8) | (data[2 + offset] << 16));
                AddPartial(v1, v2);
                break;
            case 6:
                v1 = Read32(data, offset);
                offset += 4;
                v2 = Read16(data, offset);
                AddPartial(v1, v2);
                break;
            case 5:
                v1 = Read32(data, offset);
                offset += 4;
                v2 = data[offset];
                AddPartial(v1, v2);
                break;
            case 4:
                v1 = Read32(data, offset);
                AddPartial(v1, 0);
                break;
            case 3:
                v1 = (uint)(data[0 + offset] | (data[1 + offset] << 8) | (data[2 + offset] << 16));
                AddPartial(v1, 0);
                break;
            case 2:
                v1 = Read16(data, offset);
                AddPartial(v1, 0);
                break;
            case 1:
                v1 = data[offset];
                AddPartial(v1, 0);
                break;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddPartial(uint value1, uint value2) => sum += (value1 + FARSH_KEYS[keyindex]) * (ulong)(value2 + FARSH_KEYS[keyindex + 1]);

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