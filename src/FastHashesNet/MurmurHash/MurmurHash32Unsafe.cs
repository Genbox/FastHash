//Ported to C# by Ian Qvist
//Source: https://github.com/aappleby/smhasher/
//Note: This is the x86 version of 32bit MurmurHash3
//Note: This hash algorithm is vulnerable to hash flodding: https://emboss.github.io/blog/2012/12/14/breaking-murmur-hash-flooding-dos-reloaded/

namespace FastHashesNet.MurmurHash
{
    public static class MurmurHash32Unsafe
    {
        public static unsafe uint ComputeHash(byte* data, int length, uint seed = 0)
        {
            int nblocks = length / 4;
            uint h1 = seed;
            uint k1;

            uint* blocks = (uint*)(data + nblocks * 4);

            for (int i = -nblocks; i != 0; i++)
            {
                k1 = blocks[i];

                k1 *= MurmurHashConstants.C1_32;
                k1 = Utilities.Rotate(k1, 15);
                k1 *= MurmurHashConstants.C2_32;

                h1 ^= k1;
                h1 = Utilities.Rotate(h1, 13);
                h1 = h1 * 5 + 0xe6546b64;
            }

            byte* tail = data + nblocks * 4;
            k1 = 0;

            switch (length & 3)
            {
                case 3:
                    k1 ^= (uint)tail[2] << 16;
                    goto case 2;
                case 2:
                    k1 ^= (uint)tail[1] << 8;
                    goto case 1;
                case 1:
                    k1 ^= tail[0];
                    break;
            }

            k1 *= MurmurHashConstants.C1_32;
            k1 = Utilities.Rotate(k1, 15);
            k1 *= MurmurHashConstants.C2_32;
            h1 ^= k1;

            uint len = (uint)length;

            h1 ^= len;
            h1 = Utilities.FMix(h1);

            return h1;
        }
    }
}