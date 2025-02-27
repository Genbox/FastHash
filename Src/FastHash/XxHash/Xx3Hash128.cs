﻿using static Genbox.FastHash.XxHash.XxHashConstants;
using static Genbox.FastHash.XxHash.XxHashShared;

namespace Genbox.FastHash.XxHash;

public static class Xx3Hash128
{
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data, ulong seed = 0)
    {
        return XXH3_128bits_internal(data, data.Length, seed, kSecret, SECRET_DEFAULT_SIZE, XXH3_hashLong_128b_withSeed);
    }

    private static UInt128 XXH3_hashLong_128b_withSeed(ReadOnlySpan<byte> input, int len, ulong seed64, ReadOnlySpan<byte> secret, int secretLen) => XXH3_hashLong_128b_withSeed_internal(input, len, seed64, secret, secretLen, XXH3_accumulate_512_scalar, XXH3_scrambleAcc_scalar, XXH3_initCustomSecret_scalar);

    private static UInt128 XXH3_128bits_internal(ReadOnlySpan<byte> input, int len, ulong seed64, ReadOnlySpan<byte> secret, int secretLen, XXH3_hashLong128_f f_hl128)
    {
        //XXH_ASSERT(secretLen >= XXH3_SECRET_SIZE_MIN);

        if (len <= 16)
            return XXH3_len_0to16_128b(input, len, secret, seed64);
        if (len <= 128)
            return XXH3_len_17to128_128b(input, len, secret, seed64);
        if (len <= MIDSIZE_MAX)
            return XXH3_len_129to240_128b(input, len, secret, seed64);
        return f_hl128(input, len, seed64, secret, secretLen);
    }

    private static UInt128 XXH3_hashLong_128b_withSeed_internal(ReadOnlySpan<byte> input, int len, ulong seed64, ReadOnlySpan<byte> secret, int secretlen, XXH3_f_accumulate_512 f_acc512, XXH3_f_scrambleAcc f_scramble, XXH3_f_initCustomSecret f_initSec)
    {
        if (seed64 == 0)
            return XXH3_hashLong_128b_internal(input, len, secret, secretlen, f_acc512, f_scramble);

        Span<byte> customSecret = stackalloc byte[SECRET_DEFAULT_SIZE];
        f_initSec(customSecret, seed64);
        return XXH3_hashLong_128b_internal(input, len, customSecret, SECRET_DEFAULT_SIZE, f_acc512, f_scramble);
    }

    private static UInt128 XXH3_hashLong_128b_internal(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, int secretSize, XXH3_f_accumulate_512 f_acc512, XXH3_f_scrambleAcc f_scramble)
    {
        Span<ulong> acc = stackalloc ulong[ACC_NB];
        acc[0] = INIT_ACC[0];
        acc[1] = INIT_ACC[1];
        acc[2] = INIT_ACC[2];
        acc[3] = INIT_ACC[3];
        acc[4] = INIT_ACC[4];
        acc[5] = INIT_ACC[5];
        acc[6] = INIT_ACC[6];
        acc[7] = INIT_ACC[7];

        XXH3_hashLong_internal_loop(acc, input, len, secret, secretSize, f_acc512, f_scramble);

        UInt128 uInt128;
        uInt128.Low = XXH3_mergeAccs(acc, secret, SECRET_MERGEACCS_START, (ulong)len * PRIME64_1);
        uInt128.High = XXH3_mergeAccs(acc, secret, secretSize - ACC_SIZE - SECRET_MERGEACCS_START, ~((ulong)len * PRIME64_2));
        return uInt128;
    }

    private static UInt128 XXH3_len_0to16_128b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        if (len > 8) return XXH3_len_9to16_128b(input, len, secret, seed);
        if (len >= 4) return XXH3_len_4to8_128b(input, len, secret, seed);
        if (len != 0) return XXH3_len_1to3_128b(input, len, secret, seed);
        {
            UInt128 h128;
            ulong bitflipl = Read64(secret, 64) ^ Read64(secret, 72);
            ulong bitfliph = Read64(secret, 80) ^ Read64(secret, 88);
            h128.Low = YC_xmxmx_XXH2_64(seed ^ bitflipl);
            h128.High = YC_xmxmx_XXH2_64(seed ^ bitfliph);
            return h128;
        }
    }

    private static UInt128 XXH3_len_17to128_128b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        // XXH_ASSERT(secretSize >= XXH3_SECRET_SIZE_MIN); (void)secretSize;
        // XXH_ASSERT(16 < len && len <= 128);

        UInt128 acc;
        acc.Low = (ulong)len * PRIME64_1;
        acc.High = 0;

#if XXH_SIZE_OPT
        /* Smaller, but slightly slower. */
        size_t i = (len - 1) / 32;
        do {
            acc = XXH128_mix32B(acc, input+16*i, input+len-16*(i+1), secret+32*i, seed);
        } while (i-- != 0);
#else
        if (len > 32)
        {
            if (len > 64)
            {
                if (len > 96)
                    acc = XXH128_mix32B(acc, input, 48, input, len - 64, secret, 96, seed);

                acc = XXH128_mix32B(acc, input, 32, input, len - 48, secret, 64, seed);
            }
            acc = XXH128_mix32B(acc, input, 16, input, len - 32, secret, 32, seed);
        }
        acc = XXH128_mix32B(acc, input, 0, input, len - 16, secret, 0, seed);
#endif
        UInt128 h128;
        h128.Low = acc.Low + acc.High;
        h128.High = (acc.Low * PRIME64_1) + (acc.High * PRIME64_4) + (((ulong)len - seed) * PRIME64_2);
        h128.Low = XXH3_avalanche(h128.Low);
        h128.High = 0 - XXH3_avalanche(h128.High);
        return h128;
    }

    private static UInt128 XXH3_len_9to16_128b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        ulong bitflipl = (Read64(secret, 32) ^ Read64(secret, 40)) - seed;
        ulong bitfliph = (Read64(secret, 48) ^ Read64(secret, 56)) + seed;
        ulong input_lo = Read64(input);
        ulong input_hi = Read64(input, len - 8);
        UInt128 m128 = XXH_mult64to128(input_lo ^ input_hi ^ bitflipl, PRIME64_1);

        /*
         * Put len in the middle of m128 to ensure that the length gets mixed to
         * both the low and high bits in the 128x64 multiply below.
         */
        m128.Low += (ulong)(len - 1) << 54;
        input_hi ^= bitfliph;

        /*
        * Add the high 32 bits of input_hi to the high 32 bits of m128, then
        * add the long product of the low 32 bits of input_hi and XXH_PRIME32_2 to
        * the high 64 bits of m128.
        *
        * The best approach to this operation is different on 32-bit and 64-bit.
        */
#if ARCH32
        m128.High += (input_hi & 0xFFFFFFFF00000000ULL) + xxHashShared.XXH_mult32to64((uint)input_hi, XXH_PRIME32_2);
#else
        m128.High += input_hi + XXH_mult32to64((uint)input_hi, PRIME32_2 - 1);
#endif

        m128.Low ^= ByteSwap(m128.High);

        UInt128 h128 = XXH_mult64to128(m128.Low, PRIME64_2);
        h128.High += m128.High * PRIME64_2;

        h128.Low = XXH3_avalanche(h128.Low);
        h128.High = XXH3_avalanche(h128.High);
        return h128;
    }

    private static UInt128 XXH3_len_1to3_128b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        /* A doubled version of 1to3_64b with different constants. */
        //XXH_ASSERT(input != NULL);
        //XXH_ASSERT(1 <= len && len <= 3);
        //XXH_ASSERT(secret != NULL);

        /*
         * len = 1: combinedl = { input[0], 0x01, input[0], input[0] }
         * len = 2: combinedl = { input[1], 0x02, input[0], input[1] }
         * len = 3: combinedl = { input[2], 0x03, input[0], input[1] }
         */

        byte c1 = input[0];
        byte c2 = input[len >> 1];
        byte c3 = input[len - 1];

        uint combinedl = ((uint)c1 << 16) | ((uint)c2 << 24) | ((uint)c3 << 0) | ((uint)len << 8);
        uint combinedh = RotateLeft(ByteSwap(combinedl), 13);

        ulong bitflipl = (Read32(secret) ^ Read32(secret, 4)) + seed;
        ulong bitfliph = (Read32(secret, 8) ^ Read32(secret, 12)) - seed;
        ulong keyed_lo = combinedl ^ bitflipl;
        ulong keyed_hi = combinedh ^ bitfliph;
        UInt128 h128;
        h128.Low = YC_xmxmx_XXH2_64(keyed_lo);
        h128.High = YC_xmxmx_XXH2_64(keyed_hi);
        return h128;
    }

    private static UInt128 XXH3_len_4to8_128b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        // XXH_ASSERT(input != NULL);
        // XXH_ASSERT(secret != NULL);
        // XXH_ASSERT(4 <= len && len <= 8);

        seed ^= (ulong)ByteSwap((uint)seed) << 32;

        uint input_lo = Read32(input);
        uint input_hi = Read32(input, len - 4);
        ulong input_64 = input_lo + ((ulong)input_hi << 32);
        ulong bitflip = (Read64(secret, 16) ^ Read64(secret, 24)) + seed;
        ulong keyed = input_64 ^ bitflip;

        UInt128 m128 = XXH_mult64to128(keyed, PRIME64_1 + ((ulong)len << 2));

        m128.High += m128.Low << 1;
        m128.Low ^= m128.High >> 3;

        m128.Low = XXH_xorshift64(m128.Low, 35);
        m128.Low *= 0x9FB21C651E98DF25UL;
        m128.Low = XXH_xorshift64(m128.Low, 28);
        m128.High = XXH3_avalanche(m128.High);
        return m128;
    }

    private static UInt128 XXH3_len_129to240_128b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        //XXH_ASSERT(secretSize >= XXH3_SECRET_SIZE_MIN); (void)secretSize;
        //XXH_ASSERT(128 < len && len <= XXH3_MIDSIZE_MAX);

        UInt128 acc;
        int nbRounds = len / 32;
        int i;
        acc.Low = (ulong)len * PRIME64_1;
        acc.High = 0;

        for (i = 0; i < 4; i++)
            acc = XXH128_mix32B(acc, input, 32 * i, input, (32 * i) + 16, secret, 32 * i, seed);

        acc.Low = XXH3_avalanche(acc.Low);
        acc.High = XXH3_avalanche(acc.High);

        for (i = 4; i < nbRounds; i++)
            acc = XXH128_mix32B(acc, input, 32 * i, input, (32 * i) + 16, secret, MIDSIZE_STARTOFFSET + (32 * (i - 4)), seed);

        /* last bytes */
        acc = XXH128_mix32B(acc,
            input, len - 16,
            input, len - 32,
            secret, SECRET_SIZE_MIN - MIDSIZE_LASTOFFSET - 16,
            0UL - seed);

        UInt128 h128;
        h128.Low = acc.Low + acc.High;
        h128.High = (acc.Low * PRIME64_1) + (acc.High * PRIME64_4) + (((ulong)len - seed) * PRIME64_2);
        h128.Low = XXH3_avalanche(h128.Low);
        h128.High = 0 - XXH3_avalanche(h128.High);
        return h128;
    }

    private static UInt128 XXH128_mix32B(UInt128 acc, ReadOnlySpan<byte> input_1, int offset1, ReadOnlySpan<byte> input_2, int offset2, ReadOnlySpan<byte> secret, int secretOffset, ulong seed)
    {
        acc.Low += XXH3_mix16B(input_1, offset1, secret, secretOffset + 0, seed);
        acc.Low ^= Read64(input_2, offset2) + Read64(input_2, offset2 + 8);
        acc.High += XXH3_mix16B(input_2, offset2, secret, secretOffset + 16, seed);
        acc.High ^= Read64(input_1, offset1) + Read64(input_1, offset1 + 8);
        return acc;
    }
}