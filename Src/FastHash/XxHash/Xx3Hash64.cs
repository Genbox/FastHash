using System.Runtime.CompilerServices;
using static Genbox.FastHash.XxHash.XxHashConstants;
using static Genbox.FastHash.XxHash.XxHashShared;

namespace Genbox.FastHash.XxHash;

public static class Xx3Hash64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed = 0)
    {
        seed ^= (ulong)ByteSwap((uint)seed) << 32;

        uint input1 = (uint)input;
        uint input2 = (uint)(input >> 32);
        ulong bitflip = (Read64(kSecret, 8) ^ Read64(kSecret, 16)) - seed;
        ulong input64 = input2 + ((ulong)input1 << 32);
        ulong keyed = input64 ^ bitflip;
        return XXH3_rrmxmx(keyed, 8);
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed = 0)
    {
        int length = data.Length;
        return XXH3_64bits_internal(data, length, seed, kSecret, SECRET_DEFAULT_SIZE, XXH3_hashLong_64b_withSeed);
    }

    private static ulong XXH3_hashLong_64b_withSeed_internal(ReadOnlySpan<byte> input, int len, ulong seed, XXH3_f_accumulate_512 f_acc512, XXH3_f_scrambleAcc f_scramble, XXH3_f_initCustomSecret f_initSec)
    {
        if (seed == 0)
            return XXH3_hashLong_64b_internal(input, len, kSecret, SECRET_DEFAULT_SIZE, f_acc512, f_scramble);

        Span<byte> secret = stackalloc byte[SECRET_DEFAULT_SIZE];
        f_initSec(secret, seed);
        return XXH3_hashLong_64b_internal(input, len, secret, SECRET_DEFAULT_SIZE, f_acc512, f_scramble);
    }

    private static ulong XXH3_hashLong_64b_withSeed(ReadOnlySpan<byte> input, int len, ulong seed, ReadOnlySpan<byte> secret, int secretLen)
    {
        return XXH3_hashLong_64b_withSeed_internal(input, len, seed, XXH3_accumulate_512_scalar, XXH3_scrambleAcc_scalar, XXH3_initCustomSecret_scalar);
    }

    private static ulong XXH3_hashLong_64b_internal(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, int secretSize, XXH3_f_accumulate_512 f_acc512, XXH3_f_scrambleAcc f_scramble)
    {
        Span<ulong> acc = stackalloc ulong[ACC_NB * 8];
        acc[0] = INIT_ACC[0];
        acc[1] = INIT_ACC[1];
        acc[2] = INIT_ACC[2];
        acc[3] = INIT_ACC[3];
        acc[4] = INIT_ACC[4];
        acc[5] = INIT_ACC[5];
        acc[6] = INIT_ACC[6];
        acc[7] = INIT_ACC[7];

        XXH3_hashLong_internal_loop(acc, input, len, secret, secretSize, f_acc512, f_scramble);
        ulong res = XXH3_mergeAccs(acc, secret, SECRET_MERGEACCS_START, (ulong)len * PRIME64_1);
        return res;
    }

    private static ulong XXH3_64bits_internal(ReadOnlySpan<byte> input, int len, ulong seed64, ReadOnlySpan<byte> secret, int secretLen, XXH3_hashLong64_f f_hashLong)
    {
        // XXH_ASSERT(secretLen >= XXH3_SECRET_SIZE_MIN);

        if (len <= 16)
            return XXH3_len_0to16_64b(input, len, secret, seed64);
        if (len <= 128)
            return XXH3_len_17to128_64b(input, len, secret, seed64);
        if (len <= MIDSIZE_MAX)
            return XXH3_len_129to240_64b(input, len, secret, seed64);

        return f_hashLong(input, len, seed64, secret, secretLen);
    }

    private static ulong XXH3_len_0to16_64b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        // XXH_ASSERT(len <= 16);

        if (len > 8)
            return XXH3_len_9to16_64b(input, len, secret, seed);
        if (len >= 4)
            return XXH3_len_4to8_64b(input, len, secret, seed);
        if (len > 0)
            return XXH3_len_1to3_64b(input, len, secret, seed);

        return YC_xmxmx_XXH2_64(seed ^ Read64(secret, 56) ^ Read64(secret, 64));
    }

    private static ulong XXH3_len_9to16_64b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        // XXH_ASSERT(input != NULL);
        // XXH_ASSERT(secret != NULL);
        // XXH_ASSERT(9 <= len && len <= 16);

        ulong bitflip1 = (Read64(secret, 24) ^ Read64(secret, 32)) + seed;
        ulong bitflip2 = (Read64(secret, 40) ^ Read64(secret, 48)) - seed;
        ulong input_lo = Read64(input) ^ bitflip1;
        ulong input_hi = Read64(input, len - 8) ^ bitflip2;
        ulong acc = (ulong)len
                    + ByteSwap(input_lo) + input_hi
                    + XXH3_mul128_fold64(input_lo, input_hi);
        return XXH3_avalanche(acc);
    }

    private static ulong XXH3_len_4to8_64b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        // XXH_ASSERT(input != NULL);
        // XXH_ASSERT(secret != NULL);
        // XXH_ASSERT(4 <= len && len <= 8);

        seed ^= (ulong)ByteSwap((uint)seed) << 32;

        uint input1 = Read32(input);
        uint input2 = Read32(input, len - 4);
        ulong bitflip = (Read64(secret, 8) ^ Read64(secret, 16)) - seed;
        ulong input64 = input2 + ((ulong)input1 << 32);
        ulong keyed = input64 ^ bitflip;
        return XXH3_rrmxmx(keyed, len);
    }

    private static ulong XXH3_rrmxmx(ulong h64, int len)
    {
        /* this mix is inspired by Pelle Evensen's rrmxmx */
        h64 ^= RotateLeft(h64, 49) ^ RotateLeft(h64, 24);
        h64 *= 0x9FB21C651E98DF25UL;
        h64 ^= (h64 >> 35) + (ulong)len;
        h64 *= 0x9FB21C651E98DF25UL;
        return XXH_xorshift64(h64, 28);
    }

    private static ulong XXH3_len_1to3_64b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        // XXH_ASSERT(input != NULL);
        // XXH_ASSERT(1 <= len && len <= 3);
        // XXH_ASSERT(secret != NULL);

        byte c1 = input[0];
        byte c2 = input[len >> 1];
        byte c3 = input[len - 1];
        uint combined = ((uint)c1 << 16) | ((uint)c2 << 24) | ((uint)c3 << 0) | ((uint)len << 8);
        ulong bitflip = (Read32(secret) ^ Read32(secret, 4)) + seed;
        ulong keyed = combined ^ bitflip;
        return YC_xmxmx_XXH2_64(keyed);
    }

    private static ulong XXH3_len_17to128_64b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        // XXH_ASSERT(secretSize >= XXH3_SECRET_SIZE_MIN); (void)secretSize;
        // XXH_ASSERT(16 < len && len <= 128);

        ulong acc = (ulong)len * PRIME64_1;

#if XXH_SIZE_OPT
        /* Smaller and cleaner, but slightly slower. */
        int i = (len - 1) / 32;
        do {
            acc += XXH3_mix16B(input+16 * i, secret+32*i, seed);
            acc += XXH3_mix16B(input+len-16*(i+1), secret+32*i+16, seed);
        } while (i-- != 0);
#else
        if (len > 32)
        {
            if (len > 64)
            {
                if (len > 96)
                {
                    acc += XXH3_mix16B(input, 48, secret, 96, seed);
                    acc += XXH3_mix16B(input, len - 64, secret, 112, seed);
                }
                acc += XXH3_mix16B(input, 32, secret, 64, seed);
                acc += XXH3_mix16B(input, len - 48, secret, 80, seed);
            }
            acc += XXH3_mix16B(input, 16, secret, 32, seed);
            acc += XXH3_mix16B(input, len - 32, secret, 48, seed);
        }

        acc += XXH3_mix16B(input, 0, secret, 0, seed);
        acc += XXH3_mix16B(input, len - 16, secret, 16, seed);
#endif
        return XXH3_avalanche(acc);
    }

    private static ulong XXH3_len_129to240_64b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        // XXH_ASSERT(secretSize >= XXH3_SECRET_SIZE_MIN); (void)secretSize;
        // XXH_ASSERT(128 < len && len <= XXH3_MIDSIZE_MAX);

        ulong acc = (ulong)len * PRIME64_1;
        int nbRounds = len / 16;
        int i;
        for (i = 0; i < 8; i++)
            acc += XXH3_mix16B(input, 16 * i, secret, 16 * i, seed);

        acc = XXH3_avalanche(acc);

        for (i = 8; i < nbRounds; i++)
            acc += XXH3_mix16B(input, 16 * i, secret, (16 * (i - 8)) + MIDSIZE_STARTOFFSET, seed);

        /* last bytes */
        acc += XXH3_mix16B(input, len - 16, secret, SECRET_SIZE_MIN - MIDSIZE_LASTOFFSET, seed);
        return XXH3_avalanche(acc);
    }
}