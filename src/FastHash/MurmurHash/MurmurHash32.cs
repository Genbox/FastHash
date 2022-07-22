//Ported to C# by Ian Qvist
//Source: https://github.com/aappleby/smhasher/
//Note: This is the x86 version of 32bit MurmurHash3
//Note: This hash algorithm is vulnerable to hash flodding: https://emboss.github.io/blog/2012/12/14/breaking-murmur-hash-flooding-dos-reloaded/

namespace Genbox.FastHash.MurmurHash;

public static class MurmurHash32
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
            k1 = Utilities.Read32(data, i);

            k1 *= MurmurHashConstants.C1_32;
            k1 = Utilities.RotateLeft(k1, 15);
            k1 *= MurmurHashConstants.C2_32;

            h1 ^= k1;
            h1 = Utilities.RotateLeft(h1, 13);
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

        k1 *= MurmurHashConstants.C1_32;
        k1 = Utilities.RotateLeft(k1, 15);
        k1 *= MurmurHashConstants.C2_32;
        h1 ^= k1;

        h1 ^= length;
        h1 = Utilities.FMix(h1);

        return h1;
    }
}