using static Genbox.FastHash.RapidHash.RapidHashConstants;
using static Genbox.FastHash.RapidHash.RapidHashShared;

namespace Genbox.FastHash.RapidHash;

public static class RapidHash64
{
    public static ulong ComputeIndex(ulong input, ulong seed = 0, ulong[]? secret = null)
    {
        secret ??= DefaultSecret;
        seed ^= RapidMix(seed ^ secret[2], secret[1]);
        seed ^= 8UL;

        ulong a = input ^ secret[1];
        ulong b = input ^ seed;

        RapidMum(ref a, ref b);
        return RapidMix(a ^ secret[7], b ^ secret[1] ^ 8UL);
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed = 0, ulong[]? secret = null)
    {
        secret ??= DefaultSecret;
        seed ^= RapidMix(seed ^ secret[2], secret[1]);

        ulong a = 0;
        ulong b = 0;
        int i = data.Length;
        int offset = 0;

        if (i <= 16)
        {
            if (i >= 4)
            {
                seed ^= (ulong)i;
                if (i >= 8)
                {
                    a = Read64(data);
                    b = Read64(data, i - 8);
                }
                else
                {
                    a = Read32(data);
                    b = Read32(data, i - 4);
                }
            }
            else if (i > 0)
            {
                a = ((ulong)data[0] << 45) | data[i - 1];
                b = data[i >> 1];
            }
        }
        else
        {
            if (i > 112)
            {
                ulong see1 = seed;
                ulong see2 = seed;
                ulong see3 = seed;
                ulong see4 = seed;
                ulong see5 = seed;
                ulong see6 = seed;

                while (i > 112)
                {
                    seed = RapidMix(Read64(data, offset) ^ secret[0], Read64(data, offset + 8) ^ seed);
                    see1 = RapidMix(Read64(data, offset + 16) ^ secret[1], Read64(data, offset + 24) ^ see1);
                    see2 = RapidMix(Read64(data, offset + 32) ^ secret[2], Read64(data, offset + 40) ^ see2);
                    see3 = RapidMix(Read64(data, offset + 48) ^ secret[3], Read64(data, offset + 56) ^ see3);
                    see4 = RapidMix(Read64(data, offset + 64) ^ secret[4], Read64(data, offset + 72) ^ see4);
                    see5 = RapidMix(Read64(data, offset + 80) ^ secret[5], Read64(data, offset + 88) ^ see5);
                    see6 = RapidMix(Read64(data, offset + 96) ^ secret[6], Read64(data, offset + 104) ^ see6);

                    offset += 112;
                    i -= 112;
                }

                seed ^= see1;
                see2 ^= see3;
                see4 ^= see5;
                seed ^= see6;
                see2 ^= see4;
                seed ^= see2;
            }

            if (i > 16)
            {
                seed = RapidMix(Read64(data, offset) ^ secret[2], Read64(data, offset + 8) ^ seed);
                if (i > 32)
                {
                    seed = RapidMix(Read64(data, offset + 16) ^ secret[2], Read64(data, offset + 24) ^ seed);
                    if (i > 48)
                    {
                        seed = RapidMix(Read64(data, offset + 32) ^ secret[1], Read64(data, offset + 40) ^ seed);
                        if (i > 64)
                        {
                            seed = RapidMix(Read64(data, offset + 48) ^ secret[1], Read64(data, offset + 56) ^ seed);
                            if (i > 80)
                            {
                                seed = RapidMix(Read64(data, offset + 64) ^ secret[2], Read64(data, offset + 72) ^ seed);
                                if (i > 96)
                                    seed = RapidMix(Read64(data, offset + 80) ^ secret[1], Read64(data, offset + 88) ^ seed);
                            }
                        }
                    }
                }
            }

            a = Read64(data, offset + i - 16) ^ (ulong)i;
            b = Read64(data, offset + i - 8);
        }

        a ^= secret[1];
        b ^= seed;
        RapidMum(ref a, ref b);
        return RapidMix(a ^ secret[7], b ^ secret[1] ^ (ulong)i);
    }
}
