using System.Runtime.InteropServices;

namespace Genbox.FastHash.xxHash;

public static class Xx3Hash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* input, int length, ulong seed = 0)
    {
        fixed (byte* secretPtr = XxHashConstants.kSecret)
            return XXH3_64bits_internal(input, length, seed, secretPtr, XxHashConstants.SECRET_DEFAULT_SIZE, XXH3_hashLong_64b_withSeed);
    }

    private static unsafe ulong XXH3_hashLong_64b_withSeed_internal(byte* input, int len, ulong seed, XxHashShared.XXH3_f_accumulate_512 f_acc512, XxHashShared.XXH3_f_scrambleAcc f_scramble, XxHashShared.XXH3_f_initCustomSecret f_initSec)
    {
        if (seed == 0)
        {
            fixed (byte* secretPtr = XxHashConstants.kSecret)
                return XXH3_hashLong_64b_internal(input, len, secretPtr, XxHashConstants.SECRET_DEFAULT_SIZE, f_acc512, f_scramble);
        }

        byte* secret = stackalloc byte[XxHashConstants.SECRET_DEFAULT_SIZE];
        f_initSec(secret, seed);
        return XXH3_hashLong_64b_internal(input, len, secret, XxHashConstants.SECRET_DEFAULT_SIZE, f_acc512, f_scramble);
    }

    private static unsafe ulong XXH3_hashLong_64b_withSeed(byte* input, int len, ulong seed, byte* secret, int secretLen) => XXH3_hashLong_64b_withSeed_internal(input, len, seed, XxHashShared.XXH3_accumulate_512, XxHashShared.XXH3_scrambleAcc, XxHashShared.XXH3_initCustomSecret);

    private static unsafe ulong XXH3_hashLong_64b_internal(byte* input, int len, byte* secret, int secretSize, XxHashShared.XXH3_f_accumulate_512 f_acc512, XxHashShared.XXH3_f_scrambleAcc f_scramble)
    {
        fixed (ulong* orgAcc = &XxHashConstants.INIT_ACC[0])
        {
            ulong* accPtr = (ulong*)NativeMemory.AlignedAlloc(XxHashConstants.ACC_NB * 8, XxHashConstants.ACC_ALIGN);
            accPtr[0] = orgAcc[0];
            accPtr[1] = orgAcc[1];
            accPtr[2] = orgAcc[2];
            accPtr[3] = orgAcc[3];
            accPtr[4] = orgAcc[4];
            accPtr[5] = orgAcc[5];
            accPtr[6] = orgAcc[6];
            accPtr[7] = orgAcc[7];

            XxHashShared.XXH3_hashLong_internal_loop(accPtr, input, len, secret, secretSize, f_acc512, f_scramble);
            ulong res = XxHashShared.XXH3_mergeAccs(accPtr, secret + XxHashConstants.SECRET_MERGEACCS_START, (ulong)len * XxHashConstants.PRIME64_1);
            NativeMemory.AlignedFree(accPtr);
            return res;
        }
    }

    private static unsafe ulong XXH3_64bits_internal(byte* input, int len, ulong seed64, byte* secret, int secretLen, XxHashShared.XXH3_hashLong64_f f_hashLong)
    {
        // XXH_ASSERT(secretLen >= XXH3_SECRET_SIZE_MIN);

        if (len <= 16)
            return XXH3_len_0to16_64b(input, len, secret, seed64);
        if (len <= 128)
            return XXH3_len_17to128_64b(input, len, secret, seed64);
        if (len <= XxHashConstants.MIDSIZE_MAX)
            return XXH3_len_129to240_64b(input, len, secret, seed64);

        return f_hashLong(input, len, seed64, secret, secretLen);
    }

    private static unsafe ulong XXH3_len_0to16_64b(byte* input, int len, byte* secret, ulong seed)
    {
        // XXH_ASSERT(len <= 16);

        if (len > 8)
            return XXH3_len_9to16_64b(input, len, secret, seed);
        if (len >= 4)
            return XXH3_len_4to8_64b(input, len, secret, seed);
        if (len > 0)
            return XXH3_len_1to3_64b(input, len, secret, seed);

        return XxHashShared.XXH64_avalanche(seed ^ Utilities.Read64(secret + 56) ^ Utilities.Read64(secret + 64));
    }

    private static unsafe ulong XXH3_len_9to16_64b(byte* input, int len, byte* secret, ulong seed)
    {
        // XXH_ASSERT(input != NULL);
        // XXH_ASSERT(secret != NULL);
        // XXH_ASSERT(9 <= len && len <= 16);

        ulong bitflip1 = (Utilities.Read64(secret + 24) ^ Utilities.Read64(secret + 32)) + seed;
        ulong bitflip2 = (Utilities.Read64(secret + 40) ^ Utilities.Read64(secret + 48)) - seed;
        ulong input_lo = Utilities.Read64(input) ^ bitflip1;
        ulong input_hi = Utilities.Read64(input + len - 8) ^ bitflip2;
        ulong acc = (ulong)len
                    + Utilities.XXH_swap64(input_lo) + input_hi
                    + XxHashShared.XXH3_mul128_fold64(input_lo, input_hi);
        return XxHashShared.XXH3_avalanche(acc);
    }

    private static unsafe ulong XXH3_len_4to8_64b(byte* input, int len, byte* secret, ulong seed)
    {
        // XXH_ASSERT(input != NULL);
        // XXH_ASSERT(secret != NULL);
        // XXH_ASSERT(4 <= len && len <= 8);

        seed ^= (ulong)Utilities.XXH_swap32((uint)seed) << 32;

        uint input1 = Utilities.Read32(input);
        uint input2 = Utilities.Read32(input + len - 4);
        ulong bitflip = (Utilities.Read64(secret + 8) ^ Utilities.Read64(secret + 16)) - seed;
        ulong input64 = input2 + ((ulong)input1 << 32);
        ulong keyed = input64 ^ bitflip;
        return XXH3_rrmxmx(keyed, len);
    }

    private static ulong XXH3_rrmxmx(ulong h64, int len)
    {
        /* this mix is inspired by Pelle Evensen's rrmxmx */
        h64 ^= Utilities.RotateLeft(h64, 49) ^ Utilities.RotateLeft(h64, 24);
        h64 *= 0x9FB21C651E98DF25UL;
        h64 ^= (h64 >> 35) + (ulong)len;
        h64 *= 0x9FB21C651E98DF25UL;
        return XxHashShared.XXH_xorshift64(h64, 28);
    }

    private static unsafe ulong XXH3_len_1to3_64b(byte* input, int len, byte* secret, ulong seed)
    {
        // XXH_ASSERT(input != NULL);
        // XXH_ASSERT(1 <= len && len <= 3);
        // XXH_ASSERT(secret != NULL);

        byte c1 = input[0];
        byte c2 = input[len >> 1];
        byte c3 = input[len - 1];
        uint combined = ((uint)c1 << 16) | ((uint)c2 << 24) | ((uint)c3 << 0) | ((uint)len << 8);
        ulong bitflip = (Utilities.Read32(secret) ^ Utilities.Read32(secret + 4)) + seed;
        ulong keyed = combined ^ bitflip;
        return XxHashShared.XXH64_avalanche(keyed);
    }

    private static unsafe ulong XXH3_len_17to128_64b(byte* input, int len, byte* secret, ulong seed)
    {
        // XXH_ASSERT(secretSize >= XXH3_SECRET_SIZE_MIN); (void)secretSize;
        // XXH_ASSERT(16 < len && len <= 128);

        ulong acc = (ulong)len * XxHashConstants.PRIME64_1;

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
                    acc += XxHashShared.XXH3_mix16B(input + 48, secret + 96, seed);
                    acc += XxHashShared.XXH3_mix16B(input + len - 64, secret + 112, seed);
                }
                acc += XxHashShared.XXH3_mix16B(input + 32, secret + 64, seed);
                acc += XxHashShared.XXH3_mix16B(input + len - 48, secret + 80, seed);
            }
            acc += XxHashShared.XXH3_mix16B(input + 16, secret + 32, seed);
            acc += XxHashShared.XXH3_mix16B(input + len - 32, secret + 48, seed);
        }

        acc += XxHashShared.XXH3_mix16B(input + 0, secret + 0, seed);
        acc += XxHashShared.XXH3_mix16B(input + len - 16, secret + 16, seed);
#endif
        return XxHashShared.XXH3_avalanche(acc);
    }

    private static unsafe ulong XXH3_len_129to240_64b(byte* input, int len, byte* secret, ulong seed)
    {
        // XXH_ASSERT(secretSize >= XXH3_SECRET_SIZE_MIN); (void)secretSize;
        // XXH_ASSERT(128 < len && len <= XXH3_MIDSIZE_MAX);

        ulong acc = (ulong)len * XxHashConstants.PRIME64_1;
        int nbRounds = len / 16;
        int i;
        for (i = 0; i < 8; i++)
            acc += XxHashShared.XXH3_mix16B(input + 16 * i, secret + 16 * i, seed);

        acc = XxHashShared.XXH3_avalanche(acc);

        for (i = 8; i < nbRounds; i++)
            acc += XxHashShared.XXH3_mix16B(input + 16 * i, secret + 16 * (i - 8) + XxHashConstants.MIDSIZE_STARTOFFSET, seed);

        /* last bytes */
        acc += XxHashShared.XXH3_mix16B(input + len - 16, secret + XxHashConstants.SECRET_SIZE_MIN - XxHashConstants.MIDSIZE_LASTOFFSET, seed);
        return XxHashShared.XXH3_avalanche(acc);
    }
}