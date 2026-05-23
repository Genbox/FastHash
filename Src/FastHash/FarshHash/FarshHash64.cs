using System.Runtime.CompilerServices;
using static Genbox.FastHash.FarshHash.FarshHashConstants;

namespace Genbox.FastHash.FarshHash;

public static class FarshHash64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed = 0)
    {
        uint val1 = (uint)input;
        uint val2 = (uint)(input >> 32);

        ulong h = (val1 + 0xb8fe6c39u) * (ulong)(val2 + 0x23a44bbeu);
        uint low = farsh_final(farsh_combine(seed, h)) ^ 0xb8fe6c39u;

        h = (val1 + 0xded46de9u) * (ulong)(val2 + 0x839097dbu);
        uint high = farsh_final(farsh_combine(seed, h)) ^ 0xded46de9u;
        return low | ((ulong)high << 32);
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed = 0)
    {
        ulong lowSum = seed;
        ulong highSum = seed;
        uint length = (uint)data.Length;
        int offset = 0;

        while (length >= STRIPE)
        {
            farsh_full_block(data, offset, out ulong lowHash, out ulong highHash);
            lowSum = farsh_combine(lowSum, lowHash);
            highSum = farsh_combine(highSum, highHash);
            offset += STRIPE;
            length -= STRIPE;
        }

        if (length > 0)
        {
            farsh_partial_block(data, offset, out ulong lowHash, out ulong highHash);
            lowSum = farsh_combine(lowSum, lowHash);
            highSum = farsh_combine(highSum, highHash);
        }

        uint low = farsh_final(lowSum) ^ FARSH_KEYS[0];
        uint high = farsh_final(highSum) ^ FARSH_KEYS[4];
        return low | ((ulong)high << 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void farsh_full_block(ReadOnlySpan<byte> data, int offset, out ulong lowSum, out ulong highSum)
    {
        lowSum = 0;
        highSum = 0;

        uint j = 0;
        for (uint i = 0; i < STRIPE; i += 8, j += 2)
        {
            uint val1 = Read32(data, (uint)offset + i);
            uint val2 = Read32(data, (uint)offset + i + sizeof(uint));
            lowSum += (val1 + FARSH_KEYS[j]) * (ulong)(val2 + FARSH_KEYS[j + 1]);
            highSum += (val1 + FARSH_KEYS[j + 4]) * (ulong)(val2 + FARSH_KEYS[j + 5]);
        }
    }

    private static void farsh_partial_block(ReadOnlySpan<byte> data, int offset, out ulong lowSum, out ulong highSum)
    {
        ulong low = 0;
        ulong high = 0;
        int keyindex = 0;
        int length = data.Length;

        uint chunks = (uint)((length - offset) >> 3);

        for (; chunks > 0; chunks--)
        {
            uint val1 = Read32(data, offset);
            uint val2 = Read32(data, offset + sizeof(uint));
            low += (val1 + FARSH_KEYS[keyindex]) * (ulong)(val2 + FARSH_KEYS[keyindex + 1]);
            high += (val1 + FARSH_KEYS[keyindex + 4]) * (ulong)(val2 + FARSH_KEYS[keyindex + 5]);
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
        void AddPartial(uint v1, uint v2)
        {
            low += (v1 + FARSH_KEYS[keyindex]) * (ulong)(v2 + FARSH_KEYS[keyindex + 1]);
            high += (v1 + FARSH_KEYS[keyindex + 4]) * (ulong)(v2 + FARSH_KEYS[keyindex + 5]);
        }

        lowSum = low;
        highSum = high;
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