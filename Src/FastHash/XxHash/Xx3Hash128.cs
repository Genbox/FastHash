using System.Runtime.CompilerServices;
using static Genbox.FastHash.XxHash.XxHashConstants;
using static Genbox.FastHash.XxHash.XxHashShared;

namespace Genbox.FastHash.XxHash;

/// <summary>Computes 128-bit XXH3 hashes.</summary>
public static class Xx3Hash128
{
    /// <summary>Computes a hash for a 64-bit index using a zero seed.</summary>
    /// <param name="input">The index to hash.</param>
    /// <returns>The 128-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input)
    {
        uint inputLo = (uint)input;
        uint inputHi = (uint)(input >> 32);
        ulong input64 = inputLo + ((ulong)inputHi << 32);
        ulong keyed = input64 ^ SECRET_16_24_XOR;
        return ComputeIndexFinal(keyed);
    }

    /// <summary>Computes a hash for a 64-bit index.</summary>
    /// <param name="input">The index to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 128-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input, ulong seed)
    {
        seed ^= (ulong)ByteSwap((uint)seed) << 32;

        uint inputLo = (uint)input;
        uint inputHi = (uint)(input >> 32);
        ulong input64 = inputLo + ((ulong)inputHi << 32);
        ulong bitflip = SECRET_16_24_XOR + seed;
        ulong keyed = input64 ^ bitflip;
        return ComputeIndexFinal(keyed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 ComputeIndexFinal(ulong keyed)
    {
        UInt128 product = XXH_mult64to128(keyed, PRIME64_1 + 32UL);
        ulong high = product.High + (product.Low << 1);
        ulong low = product.Low ^ (high >> 3);
        low = XXH_xorshift64(low, 35) * 0x9FB21C651E98DF25UL;
        return new UInt128(XXH_xorshift64(low, 28), XXH3_avalanche(high));
    }

    /// <summary>Computes a hash for the supplied data using a zero seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 128-bit hash.</returns>
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    /// <summary>Computes a hash for the supplied data.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 128-bit hash.</returns>
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data, ulong seed)
    {
        int length = data.Length;
#if NET8_0_OR_GREATER
        if (length > MIDSIZE_MAX)
        {
            unsafe
            {
                fixed (byte* ptr = data)
                    return Xx3Hash128Unsafe.ComputeHash(ptr, length, seed);
            }
        }
#endif

        return XXH3_128bits_internal(data, length, seed);
    }

    private static UInt128 XXH3_hashLong_128b_withSeed(ReadOnlySpan<byte> input, int len, ulong seed64) => XXH3_hashLong_128b_withSeed_internal(input, len, seed64);

    private static UInt128 XXH3_128bits_internal(ReadOnlySpan<byte> input, int len, ulong seed64)
    {
        //XXH_ASSERT(secretLen >= XXH3_SECRET_SIZE_MIN);

        if (len <= 16)
            return XXH3_len_0to16_128b(input, len, kSecret, seed64);
        if (len <= 128)
            return XXH3_len_17to128_128b(input, len, kSecret, seed64);
        if (len <= MIDSIZE_MAX)
            return XXH3_len_129to240_128b(input, len, kSecret, seed64);
        return XXH3_hashLong_128b_withSeed(input, len, seed64);
    }

    private static UInt128 XXH3_hashLong_128b_withSeed_internal(ReadOnlySpan<byte> input, int len, ulong seed64)
    {
        if (seed64 == 0)
            return XXH3_hashLong_128b_internal(input, len, kSecret, SECRET_DEFAULT_SIZE);

        Span<byte> customSecret = stackalloc byte[SECRET_DEFAULT_SIZE];
        XXH3_initCustomSecret_scalar(customSecret, seed64);
        return XXH3_hashLong_128b_internal(input, len, customSecret, SECRET_DEFAULT_SIZE);
    }

    private static UInt128 XXH3_hashLong_128b_internal(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, int secretSize)
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

        XXH3_hashLong_internal_loop(acc, input, len, secret, secretSize);

        return new UInt128(XXH3_mergeAccs(acc, secret, SECRET_MERGEACCS_START, (ulong)len * PRIME64_1),
            XXH3_mergeAccs(acc, secret, secretSize - ACC_SIZE - SECRET_MERGEACCS_START, ~((ulong)len * PRIME64_2)));
    }

    private static UInt128 XXH3_len_0to16_128b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        if (len > 8) return XXH3_len_9to16_128b(input, len, secret, seed);
        if (len >= 4) return XXH3_len_4to8_128b(input, len, secret, seed);
        if (len != 0) return XXH3_len_1to3_128b(input, len, secret, seed);
        {
            ulong bitflipl = Read64(secret, 64) ^ Read64(secret, 72);
            ulong bitfliph = Read64(secret, 80) ^ Read64(secret, 88);
            return new UInt128(YC_xmxmx_XXH_64(seed ^ bitflipl), YC_xmxmx_XXH_64(seed ^ bitfliph));
        }
    }

    private static UInt128 XXH3_len_17to128_128b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        // XXH_ASSERT(secretSize >= XXH3_SECRET_SIZE_MIN); (void)secretSize;
        // XXH_ASSERT(16 < len && len <= 128);

        UInt128 acc = new UInt128((ulong)len * PRIME64_1, 0);

#if XXH_SIZE_OPT
        /* Smaller, but slightly slower. */
        size_t i = (len - 1) / 32;

        do
        {
            acc = XXH128_mix32B(acc, input + 16 * i, input + len - 16 * (i + 1), secret + 32 * i, seed);
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
        ulong low = acc.Low + acc.High;
        ulong high = (acc.Low * PRIME64_1) + (acc.High * PRIME64_4) + (((ulong)len - seed) * PRIME64_2);
        return new UInt128(XXH3_avalanche(low), 0 - XXH3_avalanche(high));
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
        m128 = new UInt128(m128.Low + ((ulong)(len - 1) << 54), m128.High);
        input_hi ^= bitfliph;

        /*
         * Add the high 32 bits of input_hi to the high 32 bits of m128, then
         * add the long product of the low 32 bits of input_hi and XXH_PRIME32_2 to
         * the high 64 bits of m128.
         *
         * The best approach to this operation is different on 32-bit and 64-bit.
         */
#if ARCH32
        m128 = new UInt128(m128.Low, m128.High + (input_hi & 0xFFFFFFFF00000000ULL) + xxHashShared.XXH_mult32to64((uint)input_hi, XXH_PRIME32_2));
#else
        m128 = new UInt128(m128.Low, m128.High + input_hi + XXH_mult32to64((uint)input_hi, PRIME32_2 - 1));
#endif

        m128 = new UInt128(m128.Low ^ ByteSwap(m128.High), m128.High);

        UInt128 h128 = XXH_mult64to128(m128.Low, PRIME64_2);
        return new UInt128(XXH3_avalanche(h128.Low), XXH3_avalanche(h128.High + (m128.High * PRIME64_2)));
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
        return new UInt128(YC_xmxmx_XXH_64(keyed_lo), YC_xmxmx_XXH_64(keyed_hi));
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

        UInt128 product = XXH_mult64to128(keyed, PRIME64_1 + ((ulong)len << 2));
        ulong high = product.High + (product.Low << 1);
        ulong low = product.Low ^ (high >> 3);
        low = XXH_xorshift64(low, 35) * 0x9FB21C651E98DF25UL;
        return new UInt128(XXH_xorshift64(low, 28), XXH3_avalanche(high));
    }

    private static UInt128 XXH3_len_129to240_128b(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, ulong seed)
    {
        //XXH_ASSERT(secretSize >= XXH3_SECRET_SIZE_MIN); (void)secretSize;
        //XXH_ASSERT(128 < len && len <= XXH3_MIDSIZE_MAX);

        UInt128 acc = new UInt128((ulong)len * PRIME64_1, 0);
        int nbRounds = len / 32;
        int i;

        for (i = 0; i < 4; i++)
            acc = XXH128_mix32B(acc, input, 32 * i, input, (32 * i) + 16, secret, 32 * i, seed);

        acc = new UInt128(XXH3_avalanche(acc.Low), XXH3_avalanche(acc.High));

        for (i = 4; i < nbRounds; i++)
            acc = XXH128_mix32B(acc, input, 32 * i, input, (32 * i) + 16, secret, MIDSIZE_STARTOFFSET + (32 * (i - 4)), seed);

        /* last bytes */
        acc = XXH128_mix32B(acc,
            input, len - 16,
            input, len - 32,
            secret, SECRET_SIZE_MIN - MIDSIZE_LASTOFFSET - 16,
            0UL - seed);

        ulong low = acc.Low + acc.High;
        ulong high = (acc.Low * PRIME64_1) + (acc.High * PRIME64_4) + (((ulong)len - seed) * PRIME64_2);
        return new UInt128(XXH3_avalanche(low), 0 - XXH3_avalanche(high));
    }

    private static UInt128 XXH128_mix32B(UInt128 acc, ReadOnlySpan<byte> input_1, int offset1, ReadOnlySpan<byte> input_2, int offset2, ReadOnlySpan<byte> secret, int secretOffset, ulong seed)
    {
        ulong low = acc.Low + XXH3_mix16B(input_1, offset1, secret, secretOffset, seed);
        low ^= Read64(input_2, offset2) + Read64(input_2, offset2 + 8);
        ulong high = acc.High + XXH3_mix16B(input_2, offset2, secret, secretOffset + 16, seed);
        high ^= Read64(input_1, offset1) + Read64(input_1, offset1 + 8);
        return new UInt128(low, high);
    }
}