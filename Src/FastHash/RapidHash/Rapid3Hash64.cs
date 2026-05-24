using System.Runtime.CompilerServices;
using static Genbox.FastHash.RapidHash.RapidHashConstants;
using static Genbox.FastHash.RapidHash.RapidHashShared;

namespace Genbox.FastHash.RapidHash;

public static class Rapid3Hash64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed = DefaultIndexSeed) => RapidHashShared.ComputeIndex(input, seed);

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed = 0, ulong[]? secret = null)
    {
        secret ??= DefaultSecret;
        ulong secret0 = secret[0];
        ulong secret1 = secret[1];
        ulong secret2 = secret[2];
        ulong secret3 = secret[3];
        ulong secret4 = secret[4];
        ulong secret5 = secret[5];
        ulong secret6 = secret[6];
        ulong secret7 = secret[7];

        seed = InitializeSeed(seed, secret1, secret2);

        int i = data.Length;
        int offset = 0;

        if (!TryReadSmall(data, ref seed, out ulong a, out ulong b))
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
                    seed = RapidMix(Read64(data, offset) ^ secret0, Read64(data, offset + 8) ^ seed);
                    see1 = RapidMix(Read64(data, offset + 16) ^ secret1, Read64(data, offset + 24) ^ see1);
                    see2 = RapidMix(Read64(data, offset + 32) ^ secret2, Read64(data, offset + 40) ^ see2);
                    see3 = RapidMix(Read64(data, offset + 48) ^ secret3, Read64(data, offset + 56) ^ see3);
                    see4 = RapidMix(Read64(data, offset + 64) ^ secret4, Read64(data, offset + 72) ^ see4);
                    see5 = RapidMix(Read64(data, offset + 80) ^ secret5, Read64(data, offset + 88) ^ see5);
                    see6 = RapidMix(Read64(data, offset + 96) ^ secret6, Read64(data, offset + 104) ^ see6);

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

            MixRemainingBlocks(data, offset, i, ref seed, secret1, secret2, 6);
            ReadLongTail(data, offset, i, out a, out b);
        }

        return FinalizeHash(i, a, b, seed, secret1, secret7);
    }
}