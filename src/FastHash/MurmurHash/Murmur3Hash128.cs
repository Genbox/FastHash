//Ported to C# by Ian Qvist
//Source: https://github.com/aappleby/smhasher/

namespace Genbox.FastHash.MurmurHash;

public static class Murmur3Hash128
{
    public static Uint128 ComputeHash(byte[] data, uint seed = 0)
    {
        uint length = (uint)data.Length;
        uint nblocks = length / 16;

        ulong h1 = seed;
        ulong h2 = seed;

        ulong k1;
        ulong k2;

        for (uint i = 0; i < nblocks; i++)
        {
            k1 = Utilities.Read64(data, i * 2 + 0);
            k2 = Utilities.Read64(data, i * 2 + 8);

            k1 *= MurmurHashConstants.C1_64;
            k1 = Utilities.RotateLeft(k1, 31);
            k1 *= MurmurHashConstants.C2_64;
            h1 ^= k1;

            h1 = Utilities.RotateLeft(h1, 27);
            h1 += h2;
            h1 = h1 * 5 + 0x52dce729;

            k2 *= MurmurHashConstants.C2_64;
            k2 = Utilities.RotateLeft(k2, 33);
            k2 *= MurmurHashConstants.C1_64;
            h2 ^= k2;

            h2 = Utilities.RotateLeft(h2, 31);
            h2 += h1;
            h2 = h2 * 5 + 0x38495ab5;
        }

        uint rem = length & 15;

        uint tail = length - rem;

        k1 = 0;
        k2 = 0;

        switch (rem)
        {
            case 15:
                k2 ^= (ulong)data[tail + 14] << 48;
                goto case 14;
            case 14:
                k2 ^= (ulong)data[tail + 13] << 40;
                goto case 13;
            case 13:
                k2 ^= (ulong)data[tail + 12] << 32;
                goto case 12;
            case 12:
                k2 ^= (ulong)data[tail + 11] << 24;
                goto case 11;
            case 11:
                k2 ^= (ulong)data[tail + 10] << 16;
                goto case 10;
            case 10:
                k2 ^= (ulong)data[tail + 9] << 8;
                goto case 9;
            case 9:
                k2 ^= (ulong)data[tail + 8] << 0;

                k2 *= MurmurHashConstants.C2_64;
                k2 = Utilities.RotateLeft(k2, 33);
                k2 *= MurmurHashConstants.C1_64;
                h2 ^= k2;

                goto case 8;
            case 8:
                k1 ^= (ulong)data[tail + 7] << 56;
                goto case 7;
            case 7:
                k1 ^= (ulong)data[tail + 6] << 48;
                goto case 6;
            case 6:
                k1 ^= (ulong)data[tail + 5] << 40;
                goto case 5;
            case 5:
                k1 ^= (ulong)data[tail + 4] << 32;
                goto case 4;
            case 4:
                k1 ^= (ulong)data[tail + 3] << 24;
                goto case 3;
            case 3:
                k1 ^= (ulong)data[tail + 2] << 16;
                goto case 2;
            case 2:
                k1 ^= (ulong)data[tail + 1] << 8;
                goto case 1;
            case 1:
                k1 ^= (ulong)data[0] << 0;

                k1 *= MurmurHashConstants.C1_64;
                k1 = Utilities.RotateLeft(k1, 31);
                k1 *= MurmurHashConstants.C2_64;
                h1 ^= k1;

                break;
        }

        h1 ^= length;
        h2 ^= length;

        h1 += h2;
        h2 += h1;

        h1 = Utilities.FMix(h1);
        h2 = Utilities.FMix(h2);

        h1 += h2;
        h2 += h1;

        return new Uint128(h1, h2);
    }
}