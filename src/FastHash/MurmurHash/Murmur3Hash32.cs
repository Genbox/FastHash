using System.Runtime.CompilerServices;
using static Genbox.FastHash.MurmurHash.MurmurHashConstants;

namespace Genbox.FastHash.MurmurHash;

public static class Murmur3Hash32
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        input *= C1_32;
        input = RotateLeft(input, 15);
        input *= C2_32;

        input = RotateLeft(input, 13);
        input = input * 5 + 0xe6546b64;
        input ^= 4;
        return Murmur_32(input);
    }

    public static uint ComputeHash(ReadOnlySpan<byte> data, uint seed = 0)
    {
        uint length = (uint)data.Length;
        uint nblocks = length / 4;
        uint h1 = seed;
        uint k1;

        uint end = nblocks * 4;

        for (uint i = 0; i < end; i += 4)
        {
            k1 = Read32(data, i);

            k1 *= C1_32;
            k1 = RotateLeft(k1, 15);
            k1 *= C2_32;

            h1 ^= k1;
            h1 = RotateLeft(h1, 13);
            h1 = h1 * 5 + 0xe6546b64;
        }

        uint rem = length & 3;

        int tail = (int)(length - rem);
        k1 = 0;

        switch (rem)
        {
            case 3:
                k1 ^= (uint)data[tail + 2] << 16;
                goto case 2;
            case 2:
                k1 ^= (uint)data[tail + 1] << 8;
                goto case 1;
            case 1:
                k1 ^= data[tail];
                break;
        }

        k1 *= C1_32;
        k1 = RotateLeft(k1, 15);
        k1 *= C2_32;
        h1 ^= k1;

        h1 ^= length;
        h1 = Murmur_32(h1);

        return h1;
    }
}