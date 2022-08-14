using System.Runtime.CompilerServices;
using static Genbox.FastHash.XxHash.XxHashConstants;

namespace Genbox.FastHash.XxHash;

public static class Xx2Hash32Unsafe
{
    public static unsafe uint ComputeHash(byte* data, int length, uint seed = 0)
    {
        uint h32;

        if (length >= 16)
        {
            byte* bEnd = data + length;
            byte* limit = bEnd - 15;
            uint v1 = seed + PRIME32_1 + PRIME32_2;
            uint v2 = seed + PRIME32_2;
            uint v3 = seed + 0;
            uint v4 = seed - PRIME32_1;

            do
            {
                v1 = Round(v1, Read32(data));
                data += 4;
                v2 = Round(v2, Read32(data));
                data += 4;
                v3 = Round(v3, Read32(data));
                data += 4;
                v4 = Round(v4, Read32(data));
                data += 4;
            } while (data < limit);

            h32 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
        }
        else
            h32 = seed + PRIME32_5;

        h32 += (uint)length;
        length &= 15;
        while (length >= 4)
        {
            h32 += Read32(data) * PRIME32_3;
            data += 4;
            h32 = RotateLeft(h32, 17) * PRIME32_4;
            length -= 4;
        }

        while (length > 0)
        {
            h32 += Read8(data) * PRIME32_5;
            data++;
            h32 = RotateLeft(h32, 11) * PRIME32_1;
            length--;
        }

        h32 ^= h32 >> 15;
        h32 *= PRIME32_2;
        h32 ^= h32 >> 13;
        h32 *= PRIME32_3;
        h32 ^= h32 >> 16;

        return h32;
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