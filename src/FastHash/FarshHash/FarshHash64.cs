using System.Runtime.CompilerServices;
using static Genbox.FastHash.FarshHash.FarshHashConstants;

namespace Genbox.FastHash.FarshHash;

public static class FarshHash64
{
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed = 0)
    {
        ulong sum = seed;
        uint length = (uint)data.Length;
        int offset = 0;

        while (length >= STRIPE)
        {
            ulong h = farsh_full_block(data, offset);
            sum = farsh_combine(sum, h);
            offset += STRIPE;
            length -= STRIPE;
        }

        if (length > 0)
        {
            ulong h = farsh_partial_block(data, offset);
            sum = farsh_combine(sum, h);
        }

        return farsh_final(sum) ^ FARSH_KEYS[0]; /* ensure that zeroes at the end of data will affect the hash value */
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong farsh_full_block(ReadOnlySpan<byte> data, int offset)
    {
        // STRIPE bytes of key material plus extra keys for hashes up to 1024 bits long

        ulong sum = 0;
        uint i;
        uint j = 0;
        for (i = 0; i < STRIPE; i += 8, j += 2)
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
                sum += (v1 + FARSH_KEYS[keyindex]) * (ulong)(v2 + FARSH_KEYS[keyindex + 1]);
                break;
            case 6:
                v1 = Read32(data, offset);
                offset += 4;
                v2 = Read16(data, offset);
                sum += (v1 + FARSH_KEYS[keyindex]) * (ulong)(v2 + FARSH_KEYS[keyindex + 1]);
                break;
            case 5:
                v1 = Read32(data, offset);
                offset += 4;
                v2 = data[offset];
                sum += (v1 + FARSH_KEYS[keyindex]) * (ulong)(v2 + FARSH_KEYS[keyindex + 1]);
                break;
            case 4:
                v1 = Read32(data, offset);
                sum += (v1 + FARSH_KEYS[keyindex]) * (ulong)FARSH_KEYS[keyindex + 1];
                break;
            case 3:
                v1 = (uint)(data[0 + offset] | (data[1 + offset] << 8) | (data[2 + offset] << 16));
                sum += (v1 + FARSH_KEYS[keyindex]) * (ulong)FARSH_KEYS[keyindex + 1];
                break;
            case 2:
                v1 = Read16(data, offset);
                sum += (v1 + FARSH_KEYS[keyindex]) * (ulong)FARSH_KEYS[keyindex + 1];
                break;
            case 1:
                v1 = data[offset];
                sum += (v1 + FARSH_KEYS[keyindex]) * (ulong)FARSH_KEYS[keyindex + 1];
                break;
        }

        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong farsh_combine(ulong sum, ulong h)
    {
        h *= PRIME64_2;
        h += h >> 31;
        h *= PRIME64_1;
        sum ^= h;
        sum = (sum + (sum >> 27)) * PRIME64_1 + PRIME64_4;
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