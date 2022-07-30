//Ported to C# by Ian Qvist
//Source: https://github.com/aappleby/smhasher/

using static Genbox.FastHash.MurmurHash.MurmurHashConstants;
using static Genbox.FastHash.MurmurHash.MurmurShared;

namespace Genbox.FastHash.MurmurHash;

public static class Murmur3Hash32
{
    public static uint ComputeHash(byte[] data, uint seed = 0)
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

        uint tail = length - rem;
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
        h1 = MurmurMix(h1);

        return h1;
    }
}