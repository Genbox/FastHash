// C# port of Cyan4973/xxHash 0.8.3 (e626a72). Copyright (c) 2012-2021 Yann Collet.
// Distributed under the BSD 2-Clause license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Genbox.FastHash.XxHash.XxHashConstants;
using static Genbox.FastHash.XxHash.XxHashShared;

namespace Genbox.FastHash.XxHash;

/// <summary>Computes 64-bit XXH3 hashes.</summary>
public static class Xx3Hash64
{
    /// <summary>Computes a hash for a 64-bit index using a zero seed.</summary>
    /// <param name="input">The index to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        uint input1 = (uint)input;
        uint input2 = (uint)(input >> 32);
        ulong input64 = input2 + ((ulong)input1 << 32);
        ulong keyed = input64 ^ SECRET_08_16_XOR;
        return XXH3_rrmxmx(keyed, 8);
    }

    /// <summary>Computes a hash for a 64-bit index.</summary>
    /// <param name="input">The index to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed)
    {
        seed ^= (ulong)ByteSwap((uint)seed) << 32;

        uint input1 = (uint)input;
        uint input2 = (uint)(input >> 32);
        ulong bitflip = SECRET_08_16_XOR - seed;
        ulong input64 = input2 + ((ulong)input1 << 32);
        ulong keyed = input64 ^ bitflip;
        return XXH3_rrmxmx(keyed, 8);
    }

    /// <summary>Computes a hash for the supplied data using a zero seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    /// <summary>Computes a hash for the supplied data.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed)
    {
        int length = data.Length;
        if (length <= 16)
            return XXH3_len_0to16_64b(data, length, seed);
        if (length <= 128)
            return XXH3_len_17to128_64b(data, length, seed);
        if (length <= MIDSIZE_MAX)
            return XXH3_len_129to240_64b(data, length, seed);
        return XXH3_hashLong_64b_withSeed(data, length, seed);
    }

    private static ulong XXH3_hashLong_64b_withSeed(ReadOnlySpan<byte> input, int len, ulong seed)
    {
        if (seed == 0)
            return XXH3_hashLong_64b_internal(input, len, kSecret, SECRET_DEFAULT_SIZE);

        Span<byte> secret = stackalloc byte[SECRET_DEFAULT_SIZE];
        XXH3_initCustomSecret_scalar(secret, seed);
        return XXH3_hashLong_64b_internal(input, len, secret, SECRET_DEFAULT_SIZE);
    }

    private static ulong XXH3_hashLong_64b_internal(ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, int secretSize)
    {
        Span<ulong> acc = stackalloc ulong[ACC_NB];
        INIT_ACC.AsSpan().CopyTo(acc);

        XXH3_hashLong_internal_loop(acc, input, len, secret, secretSize);
        ulong res = XXH3_mergeAccs(acc, secret, SECRET_MERGEACCS_START, (ulong)len * PRIME64_1);
        return res;
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

#if NET5_0_OR_GREATER
    [SkipLocalsInit]
#endif
    private static ulong XXH3_len_17to128_64b(ReadOnlySpan<byte> input, int len, ulong seed)
    {
        // XXH_ASSERT(16 < len && len <= 128);

        ulong acc = (ulong)len * PRIME64_1;

        if (len > 32)
        {
            if (len > 64)
            {
                if (len > 96)
                {
                    acc += XXH3_mix16B(input, 48, SECRET64_096, SECRET64_104, seed);
                    acc += XXH3_mix16B(input, len - 64, SECRET64_112, SECRET64_120, seed);
                }

                acc += XXH3_mix16B(input, 32, SECRET64_064, SECRET64_072, seed);
                acc += XXH3_mix16B(input, len - 48, SECRET64_080, SECRET64_088, seed);
            }

            acc += XXH3_mix16B(input, 16, SECRET64_032, SECRET64_040, seed);
            acc += XXH3_mix16B(input, len - 32, SECRET64_048, SECRET64_056, seed);
        }

        acc += XXH3_mix16B(input, 0, SECRET64_000, SECRET64_008, seed);
        acc += XXH3_mix16B(input, len - 16, SECRET64_016, SECRET64_024, seed);

        return XXH3_avalanche(acc);
    }

#if NET5_0_OR_GREATER
    [SkipLocalsInit]
#endif
    private static ulong XXH3_len_129to240_64b(ReadOnlySpan<byte> input, int len, ulong seed)
    {
        ulong acc = (ulong)len * PRIME64_1;
        acc += XXH3_mix16B(input, 0, SECRET64_000, SECRET64_008, seed);
        acc += XXH3_mix16B(input, 16, SECRET64_016, SECRET64_024, seed);
        acc += XXH3_mix16B(input, 32, SECRET64_032, SECRET64_040, seed);
        acc += XXH3_mix16B(input, 48, SECRET64_048, SECRET64_056, seed);
        acc += XXH3_mix16B(input, 64, SECRET64_064, SECRET64_072, seed);
        acc += XXH3_mix16B(input, 80, SECRET64_080, SECRET64_088, seed);
        acc += XXH3_mix16B(input, 96, SECRET64_096, SECRET64_104, seed);
        acc += XXH3_mix16B(input, 112, SECRET64_112, SECRET64_120, seed);

        acc = XXH3_avalanche(acc);

        switch ((len - 128) / 16)
        {
            case 7:
                acc += XXH3_mix16B(input, 224, SECRET64_099, SECRET64_107, seed);
                goto case 6;
            case 6:
                acc += XXH3_mix16B(input, 208, SECRET64_083, SECRET64_091, seed);
                goto case 5;
            case 5:
                acc += XXH3_mix16B(input, 192, SECRET64_067, SECRET64_075, seed);
                goto case 4;
            case 4:
                acc += XXH3_mix16B(input, 176, SECRET64_051, SECRET64_059, seed);
                goto case 3;
            case 3:
                acc += XXH3_mix16B(input, 160, SECRET64_035, SECRET64_043, seed);
                goto case 2;
            case 2:
                acc += XXH3_mix16B(input, 144, SECRET64_019, SECRET64_027, seed);
                goto case 1;
            case 1:
                acc += XXH3_mix16B(input, 128, SECRET64_003, SECRET64_011, seed);
                break;
        }

        acc += XXH3_mix16B(input, len - 16, SECRET64_119, SECRET64_127, seed);
        return XXH3_avalanche(acc);
    }


    private static ulong XXH3_len_0to16_64b(ReadOnlySpan<byte> input, int len, ulong seed)
    {
        // XXH_ASSERT(len <= 16);

        if (len > 8)
            return XXH3_len_9to16_64b(input, len, seed);
        if (len >= 4)
            return XXH3_len_4to8_64b(input, len, seed);
        if (len > 0)
            return XXH3_len_1to3_64b(input, len, seed);

        return YC_xmxmx_XXH_64(seed ^ SECRET64_056 ^ SECRET64_064);
    }

    private static ulong XXH3_len_9to16_64b(ReadOnlySpan<byte> input, int len, ulong seed)
    {
        // XXH_ASSERT(input != NULL);
        // XXH_ASSERT(9 <= len && len <= 16);

        ulong bitflip1 = (SECRET64_024 ^ SECRET64_032) + seed;
        ulong bitflip2 = (SECRET64_040 ^ SECRET64_048) - seed;
        ulong input_lo = Read64(input) ^ bitflip1;
        ulong input_hi = Read64(input, len - 8) ^ bitflip2;
        ulong acc = (ulong)len
                    + ByteSwap(input_lo) + input_hi
                    + XXH3_mul128_fold64(input_lo, input_hi);
        return XXH3_avalanche(acc);
    }

    private static ulong XXH3_len_4to8_64b(ReadOnlySpan<byte> input, int len, ulong seed)
    {
        // XXH_ASSERT(input != NULL);
        // XXH_ASSERT(4 <= len && len <= 8);

        seed ^= (ulong)ByteSwap((uint)seed) << 32;

        uint input1 = Read32(input);
        uint input2 = Read32(input, len - 4);
        ulong bitflip = (SECRET64_008 ^ SECRET64_016) - seed;
        ulong input64 = input2 + ((ulong)input1 << 32);
        ulong keyed = input64 ^ bitflip;
        return XXH3_rrmxmx(keyed, len);
    }

    private static ulong XXH3_len_1to3_64b(ReadOnlySpan<byte> input, int len, ulong seed)
    {
        // XXH_ASSERT(input != NULL);
        // XXH_ASSERT(1 <= len && len <= 3);

        byte c1 = input[0];
        byte c2 = input[len >> 1];
        byte c3 = input[len - 1];
        uint combined = ((uint)c1 << 16) | ((uint)c2 << 24) | ((uint)c3 << 0) | ((uint)len << 8);
        ulong bitflip = (SECRET32_000 ^ SECRET32_004) + seed;
        ulong keyed = combined ^ bitflip;
        return YC_xmxmx_XXH_64(keyed);
    }
}