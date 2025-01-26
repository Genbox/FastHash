using System.Runtime.CompilerServices;
using static Genbox.FastHash.XxHash.XxHashConstants;

namespace Genbox.FastHash.XxHash;

public static class Xx2Hash64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        ulong acc = input * PRIME64_2;
        acc = RotateLeft(acc, 31);
        acc *= PRIME64_1;

        ulong h64 = (PRIME64_5 + 8) ^ acc;
        h64 = (RotateLeft(h64, 27) * PRIME64_1) + PRIME64_4;
        h64 ^= h64 >> 33;
        h64 *= PRIME64_2;
        h64 ^= h64 >> 29;
        h64 *= PRIME64_3;
        h64 ^= h64 >> 32;
        return h64;
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed = 0)
    {
        uint len = (uint)data.Length;
        ulong h64;
        int offset = 0;

        if (len >= 32)
        {
            uint bEnd = len;
            uint limit = bEnd - 31;
            ulong v1 = seed + PRIME64_1 + PRIME64_2;
            ulong v2 = seed + PRIME64_2;
            ulong v3 = seed + 0;
            ulong v4 = seed - PRIME64_1;

            do
            {
                v1 = Round(v1, Read64(data, offset));
                offset += 8;
                v2 = Round(v2, Read64(data, offset));
                offset += 8;
                v3 = Round(v3, Read64(data, offset));
                offset += 8;
                v4 = Round(v4, Read64(data, offset));
                offset += 8;
            } while (offset < limit);

            h64 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
            h64 = MergeRound(h64, v1);
            h64 = MergeRound(h64, v2);
            h64 = MergeRound(h64, v3);
            h64 = MergeRound(h64, v4);
        }
        else
            h64 = seed + PRIME64_5;

        h64 += len;
        len &= 31;
        while (len >= 8)
        {
            ulong k1 = Round(0, Read64(data, offset));
            offset += 8;
            h64 ^= k1;
            h64 = (RotateLeft(h64, 27) * PRIME64_1) + PRIME64_4;
            len -= 8;
        }

        if (len >= 4)
        {
            h64 ^= Read32(data, offset) * PRIME64_1;
            offset += 4;
            h64 = (RotateLeft(h64, 23) * PRIME64_2) + PRIME64_3;
            len -= 4;
        }

        while (len > 0)
        {
            h64 ^= data[offset++] * PRIME64_5;
            h64 = RotateLeft(h64, 11) * PRIME64_1;
            len--;
        }

        return YC_xmxmx_XXH2_64(h64);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Round(ulong acc, ulong input)
    {
        acc += input * PRIME64_2;
        acc = RotateLeft(acc, 31);
        acc *= PRIME64_1;
        return acc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong MergeRound(ulong acc, ulong val)
    {
        val = Round(0, val);
        acc ^= val;
        acc = (acc * PRIME64_1) + PRIME64_4;
        return acc;
    }
}