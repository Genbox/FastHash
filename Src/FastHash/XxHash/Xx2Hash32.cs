﻿using System.Runtime.CompilerServices;
using static Genbox.FastHash.XxHash.XxHashConstants;

namespace Genbox.FastHash.XxHash;

public static class Xx2Hash32
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint acc = input * PRIME32_3;
        acc += PRIME32_5;
        acc += 4;

        uint h32 = RotateLeft(acc, 17) * PRIME32_4;
        h32 ^= h32 >> 15;
        h32 *= PRIME32_2;
        h32 ^= h32 >> 13;
        h32 *= PRIME32_3;
        h32 ^= h32 >> 16;
        return h32;
    }

    public static uint ComputeHash(ReadOnlySpan<byte> data, uint seed = 0)
    {
        uint len = (uint)data.Length;
        uint h32;
        int offset = 0;

        if (len >= 16)
        {
            uint bEnd = len;
            uint limit = bEnd - 15;
            uint v1 = seed + PRIME32_1 + PRIME32_2;
            uint v2 = seed + PRIME32_2;
            uint v3 = seed + 0;
            uint v4 = seed - PRIME32_1;

            do
            {
                v1 = Round(v1, Read32(data, offset));
                offset += 4;
                v2 = Round(v2, Read32(data, offset));
                offset += 4;
                v3 = Round(v3, Read32(data, offset));
                offset += 4;
                v4 = Round(v4, Read32(data, offset));
                offset += 4;
            } while (offset < limit);

            h32 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
        }
        else
            h32 = seed + PRIME32_5;

        h32 += len;
        len &= 15;
        while (len >= 4)
        {
            h32 += Read32(data, offset) * PRIME32_3;
            offset += 4;
            h32 = RotateLeft(h32, 17) * PRIME32_4;
            len -= 4;
        }

        while (len > 0)
        {
            h32 += data[offset++] * PRIME32_5;
            h32 = RotateLeft(h32, 11) * PRIME32_1;
            len--;
        }

        return YC_xmxmx_XXH2_32(h32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Round(uint seed, uint input)
    {
        seed += input * PRIME32_2;
        seed = RotateLeft(seed, 13);
        seed *= PRIME32_1;
        return seed;
    }
}