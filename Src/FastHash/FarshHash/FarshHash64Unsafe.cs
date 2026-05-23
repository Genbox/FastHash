using System.Runtime.CompilerServices;
using static Genbox.FastHash.FarshHash.FarshHashConstants;

namespace Genbox.FastHash.FarshHash;

public static class FarshHash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed = 0)
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

    private static unsafe uint ComputeHash32(byte* data, int length, ulong seed, int keyOffset)
    {
        ulong sum = seed;

        while (length >= STRIPE)
        {
            ulong h = farsh_full_block(data, keyOffset);
            sum = farsh_combine(sum, h);
            data += STRIPE;
            length -= STRIPE;
        }

        if (length > 0)
        {
            ulong h = farsh_partial_block(data, length, keyOffset);
            sum = farsh_combine(sum, h);
        }

        return farsh_final(sum) ^ FARSH_KEYS[keyOffset]; /* ensure that zeroes at the end of data will affect the hash value */
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong farsh_full_block(byte* data, int keyOffset)
    {
        // STRIPE bytes of key material plus extra keys for hashes up to 1024 bits long
        ulong sum = 0;
        int i;

        for (i = 0; i < STRIPE_ELEMENTS; i += 2)
            sum += (Read32(data + (i * sizeof(uint))) + FARSH_KEYS[keyOffset + i]) * (ulong)(Read32(data + ((i + 1) * sizeof(uint))) + FARSH_KEYS[keyOffset + i + 1]);

        return sum;
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

    private static unsafe ulong farsh_partial_block(byte* data, int length, int keyOffset)
    {
        ulong sum = 0;
        int elements = (length / sizeof(uint)) & ~1;
        int i;

        for (i = 0; i < elements; i += 2)
        {
            sum += (Read32(data + (i * sizeof(uint))) + FARSH_KEYS[keyOffset + i]) * (ulong)(Read32(data + ((i + 1) * sizeof(uint))) + FARSH_KEYS[keyOffset + i + 1]);
            length -= 8;
        }

        data += elements * sizeof(uint);

        uint v1;
        uint v2;

        byte* ptr = data;
        i += keyOffset;

        switch (length)
        {
            case 7:
                v1 = Read32(ptr);
                ptr += 4;
                v2 = (uint)(ptr[0] | (ptr[1] << 8) | (ptr[2] << 16));
                sum += (v1 + FARSH_KEYS[i]) * (ulong)(v2 + FARSH_KEYS[i + 1]);
                break;
            case 6:
                v1 = Read32(ptr);
                ptr += 4;
                v2 = Read16(ptr);
                sum += (v1 + FARSH_KEYS[i]) * (ulong)(v2 + FARSH_KEYS[i + 1]);
                break;
            case 5:
                v1 = Read32(ptr);
                ptr += 4;
                v2 = *ptr;
                sum += (v1 + FARSH_KEYS[i]) * (ulong)(v2 + FARSH_KEYS[i + 1]);
                break;
            case 4:
                v1 = Read32(ptr);
                sum += (v1 + FARSH_KEYS[i]) * (ulong)FARSH_KEYS[i + 1];
                break;
            case 3:
                v1 = (uint)(ptr[0] | (ptr[1] << 8) | (ptr[2] << 16));
                sum += (v1 + FARSH_KEYS[i]) * (ulong)FARSH_KEYS[i + 1];
                break;
            case 2:
                v1 = Read16(ptr);
                sum += (v1 + FARSH_KEYS[i]) * (ulong)FARSH_KEYS[i + 1];
                break;
            case 1:
                v1 = *ptr;
                sum += (v1 + FARSH_KEYS[i]) * (ulong)FARSH_KEYS[i + 1];
                break;
        }

        return sum;
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