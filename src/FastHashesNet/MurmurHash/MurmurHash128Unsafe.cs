//Ported to C# by Ian Qvist
//Source: https://github.com/aappleby/smhasher/
//Note: This is the x64 version of 128bit MurmurHash3
//Note: This hash algorithm is vulnerable to hash flodding: https://emboss.github.io/blog/2012/12/14/breaking-murmur-hash-flooding-dos-reloaded/

namespace Genbox.FastHashesNet.MurmurHash;

public static class MurmurHash128Unsafe
{
    public static unsafe byte[] ComputeHash(byte* data, int length, uint seed = 0)
    {
        int nblocks = length / 16;

        ulong h1 = seed;
        ulong h2 = seed;

        ulong k1;
        ulong k2;

        ulong* blocks = (ulong*)data;

        for (int i = 0; i < nblocks; i++)
        {
            k1 = blocks[i * 2 + 0];
            k2 = blocks[i * 2 + 1];

            k1 *= MurmurHashConstants.C1_64;
            k1 = Utilities.Rotate(k1, 31);
            k1 *= MurmurHashConstants.C2_64;
            h1 ^= k1;

            h1 = Utilities.Rotate(h1, 27);
            h1 += h2;
            h1 = h1 * 5 + 0x52dce729;

            k2 *= MurmurHashConstants.C2_64;
            k2 = Utilities.Rotate(k2, 33);
            k2 *= MurmurHashConstants.C1_64;
            h2 ^= k2;

            h2 = Utilities.Rotate(h2, 31);
            h2 += h1;
            h2 = h2 * 5 + 0x38495ab5;
        }

        byte* tail = data + nblocks * 16;

        k1 = 0;
        k2 = 0;

        switch (length & 15)
        {
            case 15:
                k2 ^= (ulong)tail[14] << 48;
                goto case 14;
            case 14:
                k2 ^= (ulong)tail[13] << 40;
                goto case 13;
            case 13:
                k2 ^= (ulong)tail[12] << 32;
                goto case 12;
            case 12:
                k2 ^= (ulong)tail[11] << 24;
                goto case 11;
            case 11:
                k2 ^= (ulong)tail[10] << 16;
                goto case 10;
            case 10:
                k2 ^= (ulong)tail[9] << 8;
                goto case 9;
            case 9:
                k2 ^= (ulong)tail[8] << 0;
                goto case 8;
            case 8:
                k1 ^= (ulong)tail[7] << 56;
                goto case 7;
            case 7:
                k1 ^= (ulong)tail[6] << 48;
                goto case 6;
            case 6:
                k1 ^= (ulong)tail[5] << 40;
                goto case 5;
            case 5:
                k1 ^= (ulong)tail[4] << 32;
                goto case 4;
            case 4:
                k1 ^= (ulong)tail[3] << 24;
                goto case 3;
            case 3:
                k1 ^= (ulong)tail[2] << 16;
                goto case 2;
            case 2:
                k1 ^= (ulong)tail[1] << 8;
                goto case 1;
            case 1:
                k1 ^= (ulong)tail[0] << 0;
                break;
        }

        k2 *= MurmurHashConstants.C2_64;
        k2 = Utilities.Rotate(k2, 33);
        k2 *= MurmurHashConstants.C1_64;
        h2 ^= k2;

        k1 *= MurmurHashConstants.C1_64;
        k1 = Utilities.Rotate(k1, 31);
        k1 *= MurmurHashConstants.C2_64;
        h1 ^= k1;

        ulong len = (ulong)length;
        h1 ^= len;
        h2 ^= len;

        h1 += h2;
        h2 += h1;

        h1 = Utilities.FMix(h1);
        h2 = Utilities.FMix(h2);

        h1 += h2;
        h2 += h1;

        byte[] result = new byte[16];

        fixed (byte* ptr = result)
        {
            ulong* ptr2 = (ulong*)ptr;

            ptr2[0] = h1;
            ptr2[1] = h2;
        }

        return result;
    }
}