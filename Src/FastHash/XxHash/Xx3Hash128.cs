// C# port of Cyan4973/xxHash 0.8.3 (e626a72). Copyright (c) 2012-2021 Yann Collet.
// Distributed under the BSD 2-Clause license; see THIRD-PARTY-NOTICES.txt at the repository root.
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
        Xx3Hash128Result result = ComputeIndexFinal(keyed);
        return new UInt128(result.High, result.Low);
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
        Xx3Hash128Result result = ComputeIndexFinal(keyed);
        return new UInt128(result.High, result.Low);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Xx3Hash128Result ComputeIndexFinal(ulong keyed)
    {
        ulong productHigh = BigMul(keyed, PRIME64_1 + 32UL, out ulong productLow);
        ulong high = productHigh + (productLow << 1);
        ulong low = productLow ^ (high >> 3);
        low = XXH_xorshift64(low, 35) * 0x9FB21C651E98DF25UL;
        return new Xx3Hash128Result(XXH3_avalanche(high), XXH_xorshift64(low, 28));
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
        Xx3Hash128Result result = ComputeHashResult(data, seed);
        return new UInt128(result.High, result.Low);
    }

    private static Xx3Hash128Result ComputeHashResult(ReadOnlySpan<byte> data, ulong seed)
    {
        int length = data.Length;
        if (length <= 16)
            return XXH3_len_0to16_128b(data, length, seed);
        if (length <= 128)
            return XXH3_len_17to128_128b(data, length, seed);
        if (length <= MIDSIZE_MAX)
            return XXH3_len_129to240_128b(data, length, seed);
        return XXH3_hashLong_128b_withSeed(data, length, seed);
    }

    private static Xx3Hash128Result XXH3_hashLong_128b_withSeed(ReadOnlySpan<byte> input, int len, ulong seed64)
    {
        if (seed64 == 0)
            return XXH3_hashLong_128b_internal(input, len, kSecret, SECRET_DEFAULT_SIZE);

        Span<byte> customSecret = stackalloc byte[SECRET_DEFAULT_SIZE];
        XXH3_initCustomSecret_scalar(customSecret, seed64);
        return XXH3_hashLong_128b_internal(input, len, customSecret, SECRET_DEFAULT_SIZE);
    }

    private static Xx3Hash128Result XXH3_hashLong_128b_internal(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, int secretSize)
    {
        Span<ulong> acc = stackalloc ulong[ACC_NB];
        INIT_ACC.AsSpan().CopyTo(acc);

        XXH3_hashLong_internal_loop(acc, input, len, secret, secretSize);

        return new Xx3Hash128Result(XXH3_mergeAccs(acc, secret, secretSize - ACC_SIZE - SECRET_MERGEACCS_START, ~((ulong)len * PRIME64_2)), XXH3_mergeAccs(acc, secret, SECRET_MERGEACCS_START, (ulong)len * PRIME64_1));
    }

    private static Xx3Hash128Result XXH3_len_0to16_128b(ReadOnlySpan<byte> input, int len, ulong seed)
    {
        if (len > 8) return XXH3_len_9to16_128b(input, len, seed);
        if (len >= 4) return XXH3_len_4to8_128b(input, len, seed);
        if (len != 0) return XXH3_len_1to3_128b(input, len, seed);
        {
            ulong bitflipl = SECRET64_064 ^ SECRET64_072;
            ulong bitfliph = SECRET64_080 ^ SECRET64_088;
            return new Xx3Hash128Result(YC_xmxmx_XXH_64(seed ^ bitfliph), YC_xmxmx_XXH_64(seed ^ bitflipl));
        }
    }

#if NET5_0_OR_GREATER
    [SkipLocalsInit]
#endif
    private static Xx3Hash128Result XXH3_len_17to128_128b(ReadOnlySpan<byte> input, int len, ulong seed)
    {
        // XXH_ASSERT(16 < len && len <= 128);

        ulong low = (ulong)len * PRIME64_1;
        ulong high = 0;

        if (len > 32)
        {
            if (len > 64)
            {
                if (len > 96)
                    XXH128_mix32B(ref low, ref high, input, 48, len - 64, SECRET64_096, SECRET64_104, SECRET64_112, SECRET64_120, seed);

                XXH128_mix32B(ref low, ref high, input, 32, len - 48, SECRET64_064, SECRET64_072, SECRET64_080, SECRET64_088, seed);
            }

            XXH128_mix32B(ref low, ref high, input, 16, len - 32, SECRET64_032, SECRET64_040, SECRET64_048, SECRET64_056, seed);
        }

        XXH128_mix32B(ref low, ref high, input, 0, len - 16, SECRET64_000, SECRET64_008, SECRET64_016, SECRET64_024, seed);

        ulong resultLow = low + high;
        ulong resultHigh = (low * PRIME64_1) + (high * PRIME64_4) + (((ulong)len - seed) * PRIME64_2);
        return new Xx3Hash128Result(0 - XXH3_avalanche(resultHigh), XXH3_avalanche(resultLow));
    }

    private static Xx3Hash128Result XXH3_len_9to16_128b(ReadOnlySpan<byte> input, int len, ulong seed)
    {
        ulong bitflipl = (SECRET64_032 ^ SECRET64_040) - seed;
        ulong bitfliph = (SECRET64_048 ^ SECRET64_056) + seed;
        ulong input_lo = Read64(input);
        ulong input_hi = Read64(input, len - 8);
        ulong high = BigMul(input_lo ^ input_hi ^ bitflipl, PRIME64_1, out ulong low);
        low += (ulong)(len - 1) << 54;
        input_hi ^= bitfliph;
        high += input_hi + XXH_mult32to64((uint)input_hi, PRIME32_2 - 1);
        low ^= ByteSwap(high);
        ulong resultHigh = BigMul(low, PRIME64_2, out ulong resultLow);
        return new Xx3Hash128Result(XXH3_avalanche(resultHigh + (high * PRIME64_2)), XXH3_avalanche(resultLow));
    }

    private static Xx3Hash128Result XXH3_len_1to3_128b(ReadOnlySpan<byte> input, int len, ulong seed)
    {
        /* A doubled version of 1to3_64b with different constants. */
        //XXH_ASSERT(input != NULL);
        //XXH_ASSERT(1 <= len && len <= 3);

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

        ulong bitflipl = (SECRET32_000 ^ SECRET32_004) + seed;
        ulong bitfliph = (SECRET32_008 ^ SECRET32_012) - seed;
        ulong keyed_lo = combinedl ^ bitflipl;
        ulong keyed_hi = combinedh ^ bitfliph;
        return new Xx3Hash128Result(YC_xmxmx_XXH_64(keyed_hi), YC_xmxmx_XXH_64(keyed_lo));
    }

    private static Xx3Hash128Result XXH3_len_4to8_128b(ReadOnlySpan<byte> input, int len, ulong seed)
    {
        // XXH_ASSERT(input != NULL);
        // XXH_ASSERT(4 <= len && len <= 8);

        seed ^= (ulong)ByteSwap((uint)seed) << 32;

        uint input_lo = Read32(input);
        uint input_hi = Read32(input, len - 4);
        ulong input_64 = input_lo + ((ulong)input_hi << 32);
        ulong bitflip = (SECRET64_016 ^ SECRET64_024) + seed;
        ulong keyed = input_64 ^ bitflip;

        ulong productHigh = BigMul(keyed, PRIME64_1 + ((ulong)len << 2), out ulong productLow);
        ulong high = productHigh + (productLow << 1);
        ulong low = productLow ^ (high >> 3);
        low = XXH_xorshift64(low, 35) * 0x9FB21C651E98DF25UL;
        return new Xx3Hash128Result(XXH3_avalanche(high), XXH_xorshift64(low, 28));
    }

#if NET5_0_OR_GREATER
    [SkipLocalsInit]
#endif
    private static Xx3Hash128Result XXH3_len_129to240_128b(ReadOnlySpan<byte> input, int len, ulong seed)
    {
        ulong low = (ulong)len * PRIME64_1;
        ulong high = 0;
        XXH128_mix32B(ref low, ref high, input, 0, 16, SECRET64_000, SECRET64_008, SECRET64_016, SECRET64_024, seed);
        XXH128_mix32B(ref low, ref high, input, 32, 48, SECRET64_032, SECRET64_040, SECRET64_048, SECRET64_056, seed);
        XXH128_mix32B(ref low, ref high, input, 64, 80, SECRET64_064, SECRET64_072, SECRET64_080, SECRET64_088, seed);
        XXH128_mix32B(ref low, ref high, input, 96, 112, SECRET64_096, SECRET64_104, SECRET64_112, SECRET64_120, seed);

        low = XXH3_avalanche(low);
        high = XXH3_avalanche(high);

        if (len >= 160)
            XXH128_mix32B(ref low, ref high, input, 128, 144, SECRET64_003, SECRET64_011, SECRET64_019, SECRET64_027, seed);
        if (len >= 192)
            XXH128_mix32B(ref low, ref high, input, 160, 176, SECRET64_035, SECRET64_043, SECRET64_051, SECRET64_059, seed);
        if (len >= 224)
            XXH128_mix32B(ref low, ref high, input, 192, 208, SECRET64_067, SECRET64_075, SECRET64_083, SECRET64_091, seed);

        XXH128_mix32B(ref low, ref high, input, len - 16, len - 32, SECRET64_103, SECRET64_111, SECRET64_119, SECRET64_127, 0UL - seed);

        ulong resultLow = low + high;
        ulong resultHigh = (low * PRIME64_1) + (high * PRIME64_4) + (((ulong)len - seed) * PRIME64_2);
        return new Xx3Hash128Result(0 - XXH3_avalanche(resultHigh), XXH3_avalanche(resultLow));
    }

    private static void XXH128_mix32B(ref ulong low, ref ulong high, ReadOnlySpan<byte> input, int offset1, int offset2, ulong secret0, ulong secret1, ulong secret2, ulong secret3, ulong seed)
    {
        low += XXH3_mix16B(input, offset1, secret0, secret1, seed);
        low ^= Read64(input, offset2) + Read64(input, offset2 + 8);
        high += XXH3_mix16B(input, offset2, secret2, secret3, seed);
        high ^= Read64(input, offset1) + Read64(input, offset1 + 8);
    }
}