using static Genbox.FastHash.Misc.Utilities;
using static Genbox.FastHash.RapidHash.RapidHashConstants;
using static Genbox.FastHash.RapidHash.RapidHashShared;

namespace Genbox.FastHash.RapidHash;

public static class RapidHashNano64
{
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
            if (i > 48)
            {
                ulong see1 = seed;
                ulong see2 = seed;

                while (i > 48)
                {
                    seed = RapidMix(Read64(data, offset) ^ secret[0], Read64(data, offset + 8) ^ seed);
                    see1 = RapidMix(Read64(data, offset + 16) ^ secret[1], Read64(data, offset + 24) ^ see1);
                    see2 = RapidMix(Read64(data, offset + 32) ^ secret[2], Read64(data, offset + 40) ^ see2);

                    offset += 48;
                    i -= 48;
                }

                seed ^= see1;
                seed ^= see2;
            }

            if (i > 16)
            {
                seed = RapidMix(Read64(data, offset) ^ secret[2], Read64(data, offset + 8) ^ seed);
                if (i > 32)
                    seed = RapidMix(Read64(data, offset + 16) ^ secret[2], Read64(data, offset + 24) ^ seed);
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
