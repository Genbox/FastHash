using System.Runtime.CompilerServices;
using static Genbox.FastHash.FarshHash.FarshHashConstants;

namespace Genbox.FastHash.FarshHash;

public static class FarshHash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int length) => ComputeHash(data, length, 0);

    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed)
    {
        ulong lowSum = seed;
        ulong highSum = seed;

        while (length >= STRIPE)
        {
            farsh_full_block(data, out ulong lowHash, out ulong highHash);
            lowSum = farsh_combine(lowSum, lowHash);
            highSum = farsh_combine(highSum, highHash);
            data += STRIPE;
            length -= STRIPE;
        }

        if (length > 0)
        {
            farsh_partial_block(data, length, out ulong lowHash, out ulong highHash);
            lowSum = farsh_combine(lowSum, lowHash);
            highSum = farsh_combine(highSum, highHash);
        }

        uint low = farsh_final(lowSum) ^ FARSH_KEYS[0];
        uint high = farsh_final(highSum) ^ FARSH_KEYS[4];
        return low | ((ulong)high << 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void farsh_full_block(byte* data, out ulong lowSum, out ulong highSum)
    {
        lowSum = 0;
        highSum = 0;

        for (int i = 0; i < STRIPE_ELEMENTS; i += 2)
        {
            uint val1 = Read32(data + (i * sizeof(uint)));
            uint val2 = Read32(data + ((i + 1) * sizeof(uint)));
            lowSum += (val1 + FARSH_KEYS[i]) * (ulong)(val2 + FARSH_KEYS[i + 1]);
            highSum += (val1 + FARSH_KEYS[i + 4]) * (ulong)(val2 + FARSH_KEYS[i + 5]);
        }
    }

    private static unsafe void farsh_partial_block(byte* data, int length, out ulong lowSum, out ulong highSum)
    {
        ulong low = 0;
        ulong high = 0;
        int elements = (length / sizeof(uint)) & ~1;
        int i;

        for (i = 0; i < elements; i += 2)
        {
            uint val1 = Read32(data + (i * sizeof(uint)));
            uint val2 = Read32(data + ((i + 1) * sizeof(uint)));
            low += (val1 + FARSH_KEYS[i]) * (ulong)(val2 + FARSH_KEYS[i + 1]);
            high += (val1 + FARSH_KEYS[i + 4]) * (ulong)(val2 + FARSH_KEYS[i + 5]);
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
        void AddPartial(uint v1, uint v2)
        {
            low += (v1 + FARSH_KEYS[i]) * (ulong)(v2 + FARSH_KEYS[i + 1]);
            high += (v1 + FARSH_KEYS[i + 4]) * (ulong)(v2 + FARSH_KEYS[i + 5]);
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