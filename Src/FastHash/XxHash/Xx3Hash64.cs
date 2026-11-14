// C# port of Cyan4973/xxHash 0.8.3 (e626a72). Copyright (c) 2012-2021 Yann Collet.
// Distributed under the BSD 2-Clause license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Genbox.FastHash.XxHash.XxHashConstants;

#if NET8_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong XXH3_avalanche(ulong hash)
    {
        hash ^= hash >> 37;
        hash *= 0x165667919E3779F9UL;
        hash ^= hash >> 32;
        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong XXH_mult32to64(ulong x, ulong y) => (uint)x * (ulong)(uint)y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong XXH_xorshift64(ulong v64, int shift) => v64 ^ (v64 >> shift);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static UInt128 XXH_mult64to128(ulong lhs, ulong rhs) => Multiply64To128(lhs, rhs);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong XXH3_mul128_fold64(ulong lhs, ulong rhs) => Fold128To64(lhs, rhs);

    internal static ulong XXH3_mix16B(ReadOnlySpan<byte> input, int offset, ulong secretLow, ulong secretHigh, ulong seed)
    {
        ulong inputLow = Read64(input, offset) ^ (secretLow + seed);
        ulong inputHigh = Read64(input, offset + 8) ^ (secretHigh - seed);
        return XXH3_mul128_fold64(inputLow, inputHigh);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void XXH3_hashLong_internal_loop(Span<ulong> acc, ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, int secretSize)
    {
#if NET8_0_OR_GREATER
        if (Avx2.IsSupported)
        {
            XXH3_hashLong_internal_loop_avx2(acc, input, len, secret, secretSize);
            return;
        }
        if (Sse2.IsSupported)
        {
            XXH3_hashLong_internal_loop_sse2(acc, input, len, secret, secretSize);
            return;
        }
#endif
        XXH3_hashLong_internal_loop_scalar(acc, input, len, secret, secretSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XXH3_hashLong_internal_loop_scalar(Span<ulong> acc, ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, int secretSize)
    {
        int nbStripesPerBlock = (secretSize - STRIPE_LEN) / SECRET_CONSUME_RATE;
        int block_len = STRIPE_LEN * nbStripesPerBlock;
        int nb_blocks = (len - 1) / block_len;

        for (int n = 0; n < nb_blocks; n++)
        {
            XXH3_accumulate_scalar(acc, input, n * block_len, secret, 0, nbStripesPerBlock);
            XXH3_scrambleAcc_scalar(acc, secret, secretSize - STRIPE_LEN);
        }

        /* last partial block */
        //  XXH_ASSERT(len > XXH_STRIPE_LEN);
        int nbStripes = (len - 1 - (block_len * nb_blocks)) / STRIPE_LEN;

        // XXH_ASSERT(nbStripes <= (secretSize / XXH_SECRET_CONSUME_RATE));
        XXH3_accumulate_scalar(acc, input, nb_blocks * block_len, secret, 0, nbStripes);

        XXH3_accumulate_512_scalar(acc, input, len - STRIPE_LEN, secret, secretSize - STRIPE_LEN - SECRET_LASTACC_START);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong XXH3_mergeAccs(Span<ulong> acc, ReadOnlySpan<byte> secret, int secretOffset, ulong start)
    {
        ulong result64 = start;

        for (int i = 0; i < 4; i++)
            result64 += XXH3_mix2Accs(acc, 2 * i, secret, secretOffset + (16 * i));

        return XXH3_avalanche(result64);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong XXH3_mix2Accs(Span<ulong> acc, int offset, ReadOnlySpan<byte> secret, int secretOffset) => XXH3_mul128_fold64(acc[offset + 0] ^ Read64(secret, secretOffset), acc[offset + 1] ^ Read64(secret, secretOffset + 8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XXH3_accumulate_scalar(Span<ulong> acc, ReadOnlySpan<byte> input, int inputOffset, ReadOnlySpan<byte> secret, int secretOffset, int nbStripes)
    {
        for (int n = 0; n < nbStripes; n++)
            XXH3_accumulate_512_scalar(acc, input, inputOffset + (n * STRIPE_LEN), secret, secretOffset + (n * SECRET_CONSUME_RATE));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XXH3_accumulate_512_scalar(Span<ulong> acc, ReadOnlySpan<byte> input, int inputOffset, ReadOnlySpan<byte> secret, int secretOffset)
    {
        for (int i = 0; i < ACC_NB; i++)
            XXH3_scalarRound(acc, input, inputOffset, secret, secretOffset, i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XXH3_scalarRound(Span<ulong> acc, ReadOnlySpan<byte> input, int inputOffset, ReadOnlySpan<byte> secret, int secretOffset, int lane)
    {
        Span<ulong> xacc = acc;
        ReadOnlySpan<byte> xinput = input;
        ReadOnlySpan<byte> xsecret = secret;

        ulong data_val = Read64(xinput, inputOffset + (lane * 8));
        ulong data_key = data_val ^ Read64(xsecret, secretOffset + (lane * 8));
        xacc[lane ^ 1] += data_val;
        xacc[lane] += XXH_mult32to64(data_key & 0xFFFFFFFF, data_key >> 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XXH3_scrambleAcc_scalar(Span<ulong> acc, ReadOnlySpan<byte> secret, int secretOffset)
    {
        for (int i = 0; i < ACC_NB; i++)
            XXH3_scalarScrambleRound(acc, secret, secretOffset, i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XXH3_scalarScrambleRound(Span<ulong> acc, ReadOnlySpan<byte> secret, int secretOffset, int lane)
    {
        Span<ulong> xacc = acc;
        ReadOnlySpan<byte> xsecret = secret;

        ulong key64 = Read64(xsecret, secretOffset + (lane * 8));
        ulong acc64 = xacc[lane];
        acc64 = XXH_xorshift64(acc64, 47);
        acc64 ^= key64;
        acc64 *= PRIME32_1;
        xacc[lane] = acc64;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void XXH3_initCustomSecret_scalar(Span<byte> customSecret, ulong seed)
    {
        int nbRounds = SECRET_DEFAULT_SIZE / 16;

        for (int i = 0; i < nbRounds; i++)
        {
            ulong lo = Read64(kSecret, 16 * i) + seed;
            ulong hi = Read64(kSecret, (16 * i) + 8) - seed;
            Write64(customSecret, 16 * i, lo);
            Write64(customSecret, (16 * i) + 8, hi);
        }
    }

#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XXH3_hashLong_internal_loop_avx2(Span<ulong> acc, ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, int secretSize)
    {
        int nbStripesPerBlock = (secretSize - STRIPE_LEN) / SECRET_CONSUME_RATE;
        int block_len = STRIPE_LEN * nbStripesPerBlock;
        int nb_blocks = (len - 1) / block_len;

        // Keep the accumulators in registers across stripes and block scrambles.
        Vector256<ulong> acc0 = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(acc), (nuint)0);
        Vector256<ulong> acc1 = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(acc), (nuint)4);

        ref byte current = ref MemoryMarshal.GetReference(input);
        ref byte scrambleSecret = ref Unsafe.Add(ref MemoryMarshal.GetReference(secret), secretSize - STRIPE_LEN);

        for (int n = 0; n < nb_blocks; n++)
        {
            ref byte stripeSecret = ref MemoryMarshal.GetReference(secret);

            for (int stripe = 0; stripe < nbStripesPerBlock; stripe++)
            {
                acc0 = XXH3_accumulate_256(acc0, ref current, ref stripeSecret);
                acc1 = XXH3_accumulate_256(acc1, ref Unsafe.Add(ref current, 32), ref Unsafe.Add(ref stripeSecret, 32));

                current = ref Unsafe.Add(ref current, STRIPE_LEN);
                stripeSecret = ref Unsafe.Add(ref stripeSecret, SECRET_CONSUME_RATE);
            }

            acc0 = XXH3_scrambleAcc_256(acc0, ref scrambleSecret);
            acc1 = XXH3_scrambleAcc_256(acc1, ref Unsafe.Add(ref scrambleSecret, 32));
        }

        int nbStripes = (len - 1 - (block_len * nb_blocks)) / STRIPE_LEN;
        ref byte remainingSecret = ref MemoryMarshal.GetReference(secret);

        for (int stripe = 0; stripe < nbStripes; stripe++)
        {
            acc0 = XXH3_accumulate_256(acc0, ref current, ref remainingSecret);
            acc1 = XXH3_accumulate_256(acc1, ref Unsafe.Add(ref current, 32), ref Unsafe.Add(ref remainingSecret, 32));

            current = ref Unsafe.Add(ref current, STRIPE_LEN);
            remainingSecret = ref Unsafe.Add(ref remainingSecret, SECRET_CONSUME_RATE);
        }

        current = ref Unsafe.Add(ref MemoryMarshal.GetReference(input), len - STRIPE_LEN);
        ref byte lastSecret = ref Unsafe.Add(ref scrambleSecret, -SECRET_LASTACC_START);
        acc0 = XXH3_accumulate_256(acc0, ref current, ref lastSecret);
        acc1 = XXH3_accumulate_256(acc1, ref Unsafe.Add(ref current, 32), ref Unsafe.Add(ref lastSecret, 32));

        acc0.StoreUnsafe(ref MemoryMarshal.GetReference(acc), (nuint)0);
        acc1.StoreUnsafe(ref MemoryMarshal.GetReference(acc), (nuint)4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XXH3_hashLong_internal_loop_sse2(Span<ulong> acc, ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, int secretSize)
    {
        int nbStripesPerBlock = (secretSize - STRIPE_LEN) / SECRET_CONSUME_RATE;
        int block_len = STRIPE_LEN * nbStripesPerBlock;
        int nb_blocks = (len - 1) / block_len;

        // Keep the accumulators in registers across stripes and block scrambles.
        Vector128<ulong> acc0 = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(acc), (nuint)0);
        Vector128<ulong> acc1 = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(acc), (nuint)2);
        Vector128<ulong> acc2 = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(acc), (nuint)4);
        Vector128<ulong> acc3 = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(acc), (nuint)6);

        ref byte current = ref MemoryMarshal.GetReference(input);
        ref byte scrambleSecret = ref Unsafe.Add(ref MemoryMarshal.GetReference(secret), secretSize - STRIPE_LEN);

        for (int n = 0; n < nb_blocks; n++)
        {
            ref byte stripeSecret = ref MemoryMarshal.GetReference(secret);

            for (int stripe = 0; stripe < nbStripesPerBlock; stripe++)
            {
                acc0 = XXH3_accumulate_128(acc0, ref current, ref stripeSecret);
                acc1 = XXH3_accumulate_128(acc1, ref Unsafe.Add(ref current, 16), ref Unsafe.Add(ref stripeSecret, 16));
                acc2 = XXH3_accumulate_128(acc2, ref Unsafe.Add(ref current, 32), ref Unsafe.Add(ref stripeSecret, 32));
                acc3 = XXH3_accumulate_128(acc3, ref Unsafe.Add(ref current, 48), ref Unsafe.Add(ref stripeSecret, 48));

                current = ref Unsafe.Add(ref current, STRIPE_LEN);
                stripeSecret = ref Unsafe.Add(ref stripeSecret, SECRET_CONSUME_RATE);
            }

            acc0 = XXH3_scrambleAcc_128(acc0, ref scrambleSecret);
            acc1 = XXH3_scrambleAcc_128(acc1, ref Unsafe.Add(ref scrambleSecret, 16));
            acc2 = XXH3_scrambleAcc_128(acc2, ref Unsafe.Add(ref scrambleSecret, 32));
            acc3 = XXH3_scrambleAcc_128(acc3, ref Unsafe.Add(ref scrambleSecret, 48));
        }

        int nbStripes = (len - 1 - (block_len * nb_blocks)) / STRIPE_LEN;
        ref byte remainingSecret = ref MemoryMarshal.GetReference(secret);

        for (int stripe = 0; stripe < nbStripes; stripe++)
        {
            acc0 = XXH3_accumulate_128(acc0, ref current, ref remainingSecret);
            acc1 = XXH3_accumulate_128(acc1, ref Unsafe.Add(ref current, 16), ref Unsafe.Add(ref remainingSecret, 16));
            acc2 = XXH3_accumulate_128(acc2, ref Unsafe.Add(ref current, 32), ref Unsafe.Add(ref remainingSecret, 32));
            acc3 = XXH3_accumulate_128(acc3, ref Unsafe.Add(ref current, 48), ref Unsafe.Add(ref remainingSecret, 48));

            current = ref Unsafe.Add(ref current, STRIPE_LEN);
            remainingSecret = ref Unsafe.Add(ref remainingSecret, SECRET_CONSUME_RATE);
        }

        current = ref Unsafe.Add(ref MemoryMarshal.GetReference(input), len - STRIPE_LEN);
        ref byte lastSecret = ref Unsafe.Add(ref scrambleSecret, -SECRET_LASTACC_START);
        acc0 = XXH3_accumulate_128(acc0, ref current, ref lastSecret);
        acc1 = XXH3_accumulate_128(acc1, ref Unsafe.Add(ref current, 16), ref Unsafe.Add(ref lastSecret, 16));
        acc2 = XXH3_accumulate_128(acc2, ref Unsafe.Add(ref current, 32), ref Unsafe.Add(ref lastSecret, 32));
        acc3 = XXH3_accumulate_128(acc3, ref Unsafe.Add(ref current, 48), ref Unsafe.Add(ref lastSecret, 48));

        acc0.StoreUnsafe(ref MemoryMarshal.GetReference(acc), (nuint)0);
        acc1.StoreUnsafe(ref MemoryMarshal.GetReference(acc), (nuint)2);
        acc2.StoreUnsafe(ref MemoryMarshal.GetReference(acc), (nuint)4);
        acc3.StoreUnsafe(ref MemoryMarshal.GetReference(acc), (nuint)6);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ulong> XXH3_accumulate_256(Vector256<ulong> acc, ref byte input, ref byte secret)
    {
        Vector256<uint> data = Unsafe.ReadUnaligned<Vector256<uint>>(ref input);
        Vector256<uint> key = Unsafe.ReadUnaligned<Vector256<uint>>(ref secret);
        Vector256<uint> dataKey = Avx2.Xor(data, key);
        Vector256<ulong> product = Avx2.Multiply(dataKey, Avx2.Shuffle(dataKey, MM_SHUFFLE_0_3_0_1));
        Vector256<ulong> dataSwap = Avx2.Shuffle(data, MM_SHUFFLE_1_0_3_2).AsUInt64();
        return Avx2.Add(Avx2.Add(acc, dataSwap), product);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ulong> XXH3_scrambleAcc_256(Vector256<ulong> acc, ref byte secret)
    {
        Vector256<uint> prime32 = Vector256.Create(PRIME32_1);
        Vector256<ulong> data = Avx2.Xor(acc, Avx2.ShiftRightLogical(acc, 47));
        Vector256<uint> dataKey = Avx2.Xor(data, Unsafe.ReadUnaligned<Vector256<ulong>>(ref secret)).AsUInt32();
        Vector256<ulong> productLow = Avx2.Multiply(dataKey, prime32);
        Vector256<ulong> productHigh = Avx2.Multiply(Avx2.ShiftRightLogical(dataKey.AsUInt64(), 32).AsUInt32(), prime32);
        return Avx2.Add(productLow, Avx2.ShiftLeftLogical(productHigh, 32));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ulong> XXH3_accumulate_128(Vector128<ulong> acc, ref byte input, ref byte secret)
    {
        Vector128<uint> data = Unsafe.ReadUnaligned<Vector128<uint>>(ref input);
        Vector128<uint> key = Unsafe.ReadUnaligned<Vector128<uint>>(ref secret);
        Vector128<uint> dataKey = Sse2.Xor(data, key);
        Vector128<ulong> product = Sse2.Multiply(dataKey, Sse2.Shuffle(dataKey, MM_SHUFFLE_0_3_0_1));
        Vector128<ulong> dataSwap = Sse2.Shuffle(data, MM_SHUFFLE_1_0_3_2).AsUInt64();
        return Sse2.Add(Sse2.Add(acc, dataSwap), product);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ulong> XXH3_scrambleAcc_128(Vector128<ulong> acc, ref byte secret)
    {
        Vector128<uint> prime32 = Vector128.Create(PRIME32_1);
        Vector128<ulong> data = Sse2.Xor(acc, Sse2.ShiftRightLogical(acc, 47));
        Vector128<uint> dataKey = Sse2.Xor(data, Unsafe.ReadUnaligned<Vector128<ulong>>(ref secret)).AsUInt32();
        Vector128<ulong> productLow = Sse2.Multiply(dataKey, prime32);
        Vector128<ulong> productHigh = Sse2.Multiply(Sse2.ShiftRightLogical(dataKey.AsUInt64(), 32).AsUInt32(), prime32);
        return Sse2.Add(productLow, Sse2.ShiftLeftLogical(productHigh, 32));
    }
#endif
}