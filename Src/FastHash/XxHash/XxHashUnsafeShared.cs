#if NET8_0_OR_GREATER

// C# port of Cyan4973/xxHash 0.8.3 (e626a72). Copyright (c) 2012-2021 Yann Collet.
// Distributed under the BSD 2-Clause license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif
using System.Runtime.CompilerServices;
using static Genbox.FastHash.XxHash.XxHashConstants;
using static Genbox.FastHash.XxHash.XxHashShared;

namespace Genbox.FastHash.XxHash;

internal static class XxHashUnsafeShared
{
    internal static unsafe void XXH3_initCustomSecret(byte* customSecret, ulong seed)
    {
#if NET8_0_OR_GREATER
        if (Avx2.IsSupported)
        {
            XXH3_initCustomSecret_avx2(customSecret, seed);
            return;
        }

        if (Sse2.IsSupported)
        {
            XXH3_initCustomSecret_sse2(customSecret, seed);
            return;
        }
#endif

        XXH3_initCustomSecret_scalar(customSecret, seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe ulong XXH3_mergeAccs(ulong* acc, byte* secret, ulong start)
    {
        ulong result64 = start;

        for (int i = 0; i < 4; i++)
            result64 += XXH3_mix2Accs(acc + (2 * i), secret + (16 * i));

        return XXH3_avalanche(result64);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong XXH3_mix2Accs(ulong* acc, byte* secret) => XXH3_mul128_fold64(acc[0] ^ Read64(secret), acc[1] ^ Read64(secret + 8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe void XXH3_hashLong_internal_loop(ulong* acc, byte* input, int len, byte* secret, int secretSize)
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
    private static unsafe void XXH3_hashLong_internal_loop_scalar(ulong* acc, byte* input, int len, byte* secret, int secretSize)
    {
        int nbStripesPerBlock = (secretSize - STRIPE_LEN) / SECRET_CONSUME_RATE;
        int block_len = STRIPE_LEN * nbStripesPerBlock;
        int nb_blocks = (len - 1) / block_len;

        for (int n = 0; n < nb_blocks; n++)
        {
            XXH3_accumulate_scalar(acc, input + (n * block_len), secret, nbStripesPerBlock);
            XXH3_scrambleAcc_scalar(acc, (secret + secretSize) - STRIPE_LEN);
        }

        /* last partial block */
        //  XXH_ASSERT(len > XXH_STRIPE_LEN);
        int nbStripes = (len - 1 - (block_len * nb_blocks)) / STRIPE_LEN;

        // XXH_ASSERT(nbStripes <= (secretSize / XXH_SECRET_CONSUME_RATE));
        XXH3_accumulate_scalar(acc, input + (nb_blocks * block_len), secret, nbStripes);

        byte* p = (input + len) - STRIPE_LEN;
        XXH3_accumulate_512_scalar(acc, p, (secret + secretSize) - STRIPE_LEN - SECRET_LASTACC_START);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_accumulate_scalar(ulong* acc, byte* input, byte* secret, int nbStripes)
    {
        for (int n = 0; n < nbStripes; n++)
            XXH3_accumulate_512_scalar(acc, input + (n * STRIPE_LEN), secret + (n * SECRET_CONSUME_RATE));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_accumulate_512_scalar(ulong* acc, byte* input, byte* secret)
    {
        for (int i = 0; i < ACC_NB; i++)
            XXH3_scalarRound(acc, input, secret, i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_scalarRound(ulong* acc, byte* input, byte* secret, int lane)
    {
        ulong* xacc = acc;
        byte* xinput = input;
        byte* xsecret = secret;

        ulong data_val = Read64(xinput + (lane * 8));
        ulong data_key = data_val ^ Read64(xsecret + (lane * 8));
        xacc[lane ^ 1] += data_val;
        xacc[lane] += XXH_mult32to64(data_key & 0xFFFFFFFF, data_key >> 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_scrambleAcc_scalar(ulong* acc, byte* secret)
    {
        for (int i = 0; i < ACC_NB; i++)
            XXH3_scalarScrambleRound(acc, secret, i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_scalarScrambleRound(ulong* acc, byte* secret, int lane)
    {
        ulong* xacc = acc;
        byte* xsecret = secret;

        ulong key64 = Read64(xsecret + (lane * 8));
        ulong acc64 = xacc[lane];
        acc64 = XXH_xorshift64(acc64, 47);
        acc64 ^= key64;
        acc64 *= PRIME32_1;
        xacc[lane] = acc64;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_initCustomSecret_scalar(byte* customSecret, ulong seed)
    {
        fixed (byte* kSecretPtr = &kSecret[0])
        {
            int nbRounds = SECRET_DEFAULT_SIZE / 16;

            for (int i = 0; i < nbRounds; i++)
            {
                ulong lo = Read64(kSecretPtr + (16 * i)) + seed;
                ulong hi = Read64(kSecretPtr + (16 * i) + 8) - seed;
                Write64(customSecret + (16 * i), lo);
                Write64(customSecret + (16 * i) + 8, hi);
            }
        }
    }

    internal static unsafe ulong XXH3_mix16B(byte* input, ulong secretLow, ulong secretHigh, ulong seed)
    {
        ulong inputLow = Read64(input) ^ (secretLow + seed);
        ulong inputHigh = Read64(input + 8) ^ (secretHigh - seed);
        return XXH3_mul128_fold64(inputLow, inputHigh);
    }

#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_hashLong_internal_loop_avx2(ulong* acc, byte* input, int len, byte* secret, int secretSize)
    {
        int nbStripesPerBlock = (secretSize - STRIPE_LEN) / SECRET_CONSUME_RATE;
        int block_len = STRIPE_LEN * nbStripesPerBlock;
        int nb_blocks = (len - 1) / block_len;

        // Keep the accumulators in registers across stripes and block scrambles.
        Vector256<ulong> acc0 = Unsafe.ReadUnaligned<Vector256<ulong>>(acc + 0);
        Vector256<ulong> acc1 = Unsafe.ReadUnaligned<Vector256<ulong>>(acc + 4);

        byte* p = input;
        byte* scrambleSecret = (secret + secretSize) - STRIPE_LEN;

        for (int n = 0; n < nb_blocks; n++)
        {
            byte* stripeSecret = secret;

            for (int stripe = 0; stripe < nbStripesPerBlock; stripe++)
            {
                acc0 = XXH3_accumulate_256(acc0, p, stripeSecret);
                acc1 = XXH3_accumulate_256(acc1, p + 32, stripeSecret + 32);

                p += STRIPE_LEN;
                stripeSecret += SECRET_CONSUME_RATE;
            }

            acc0 = XXH3_scrambleAcc_256(acc0, scrambleSecret);
            acc1 = XXH3_scrambleAcc_256(acc1, scrambleSecret + 32);
        }

        int nbStripes = (len - 1 - (block_len * nb_blocks)) / STRIPE_LEN;
        byte* remainingSecret = secret;

        for (int stripe = 0; stripe < nbStripes; stripe++)
        {
            acc0 = XXH3_accumulate_256(acc0, p, remainingSecret);
            acc1 = XXH3_accumulate_256(acc1, p + 32, remainingSecret + 32);

            p += STRIPE_LEN;
            remainingSecret += SECRET_CONSUME_RATE;
        }

        p = (input + len) - STRIPE_LEN;
        byte* lastSecret = scrambleSecret - SECRET_LASTACC_START;
        acc0 = XXH3_accumulate_256(acc0, p, lastSecret);
        acc1 = XXH3_accumulate_256(acc1, p + 32, lastSecret + 32);

        Unsafe.WriteUnaligned(acc + 0, acc0);
        Unsafe.WriteUnaligned(acc + 4, acc1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_hashLong_internal_loop_sse2(ulong* acc, byte* input, int len, byte* secret, int secretSize)
    {
        int nbStripesPerBlock = (secretSize - STRIPE_LEN) / SECRET_CONSUME_RATE;
        int block_len = STRIPE_LEN * nbStripesPerBlock;
        int nb_blocks = (len - 1) / block_len;

        // Keep the accumulators in registers across stripes and block scrambles.
        Vector128<ulong> acc0 = Unsafe.ReadUnaligned<Vector128<ulong>>(acc + 0);
        Vector128<ulong> acc1 = Unsafe.ReadUnaligned<Vector128<ulong>>(acc + 2);
        Vector128<ulong> acc2 = Unsafe.ReadUnaligned<Vector128<ulong>>(acc + 4);
        Vector128<ulong> acc3 = Unsafe.ReadUnaligned<Vector128<ulong>>(acc + 6);

        byte* p = input;
        byte* scrambleSecret = (secret + secretSize) - STRIPE_LEN;

        for (int n = 0; n < nb_blocks; n++)
        {
            byte* stripeSecret = secret;

            for (int stripe = 0; stripe < nbStripesPerBlock; stripe++)
            {
                acc0 = XXH3_accumulate_128(acc0, p, stripeSecret);
                acc1 = XXH3_accumulate_128(acc1, p + 16, stripeSecret + 16);
                acc2 = XXH3_accumulate_128(acc2, p + 32, stripeSecret + 32);
                acc3 = XXH3_accumulate_128(acc3, p + 48, stripeSecret + 48);

                p += STRIPE_LEN;
                stripeSecret += SECRET_CONSUME_RATE;
            }

            acc0 = XXH3_scrambleAcc_128(acc0, scrambleSecret);
            acc1 = XXH3_scrambleAcc_128(acc1, scrambleSecret + 16);
            acc2 = XXH3_scrambleAcc_128(acc2, scrambleSecret + 32);
            acc3 = XXH3_scrambleAcc_128(acc3, scrambleSecret + 48);
        }

        int nbStripes = (len - 1 - (block_len * nb_blocks)) / STRIPE_LEN;
        byte* remainingSecret = secret;

        for (int stripe = 0; stripe < nbStripes; stripe++)
        {
            acc0 = XXH3_accumulate_128(acc0, p, remainingSecret);
            acc1 = XXH3_accumulate_128(acc1, p + 16, remainingSecret + 16);
            acc2 = XXH3_accumulate_128(acc2, p + 32, remainingSecret + 32);
            acc3 = XXH3_accumulate_128(acc3, p + 48, remainingSecret + 48);

            p += STRIPE_LEN;
            remainingSecret += SECRET_CONSUME_RATE;
        }

        p = (input + len) - STRIPE_LEN;
        byte* lastSecret = scrambleSecret - SECRET_LASTACC_START;
        acc0 = XXH3_accumulate_128(acc0, p, lastSecret);
        acc1 = XXH3_accumulate_128(acc1, p + 16, lastSecret + 16);
        acc2 = XXH3_accumulate_128(acc2, p + 32, lastSecret + 32);
        acc3 = XXH3_accumulate_128(acc3, p + 48, lastSecret + 48);

        Unsafe.WriteUnaligned(acc + 0, acc0);
        Unsafe.WriteUnaligned(acc + 2, acc1);
        Unsafe.WriteUnaligned(acc + 4, acc2);
        Unsafe.WriteUnaligned(acc + 6, acc3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Vector256<ulong> XXH3_accumulate_256(Vector256<ulong> acc, byte* input, byte* secret)
    {
        Vector256<uint> data = Unsafe.ReadUnaligned<Vector256<uint>>(input);
        Vector256<uint> key = Unsafe.ReadUnaligned<Vector256<uint>>(secret);
        Vector256<uint> dataKey = Avx2.Xor(data, key);
        Vector256<ulong> product = Avx2.Multiply(dataKey, Avx2.Shuffle(dataKey, MM_SHUFFLE_0_3_0_1));
        Vector256<ulong> dataSwap = Avx2.Shuffle(data, MM_SHUFFLE_1_0_3_2).AsUInt64();
        return Avx2.Add(Avx2.Add(acc, dataSwap), product);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Vector256<ulong> XXH3_scrambleAcc_256(Vector256<ulong> acc, byte* secret)
    {
        Vector256<uint> prime32 = Vector256.Create(PRIME32_1);
        Vector256<ulong> data = Avx2.Xor(acc, Avx2.ShiftRightLogical(acc, 47));
        Vector256<uint> dataKey = Avx2.Xor(data, Unsafe.ReadUnaligned<Vector256<ulong>>(secret)).AsUInt32();
        Vector256<ulong> productLow = Avx2.Multiply(dataKey, prime32);
        Vector256<ulong> productHigh = Avx2.Multiply(Avx2.ShiftRightLogical(dataKey.AsUInt64(), 32).AsUInt32(), prime32);
        return Avx2.Add(productLow, Avx2.ShiftLeftLogical(productHigh, 32));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Vector128<ulong> XXH3_accumulate_128(Vector128<ulong> acc, byte* input, byte* secret)
    {
        Vector128<uint> data = Unsafe.ReadUnaligned<Vector128<uint>>(input);
        Vector128<uint> key = Unsafe.ReadUnaligned<Vector128<uint>>(secret);
        Vector128<uint> dataKey = Sse2.Xor(data, key);
        Vector128<ulong> product = Sse2.Multiply(dataKey, Sse2.Shuffle(dataKey, MM_SHUFFLE_0_3_0_1));
        Vector128<ulong> dataSwap = Sse2.Shuffle(data, MM_SHUFFLE_1_0_3_2).AsUInt64();
        return Sse2.Add(Sse2.Add(acc, dataSwap), product);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Vector128<ulong> XXH3_scrambleAcc_128(Vector128<ulong> acc, byte* secret)
    {
        Vector128<uint> prime32 = Vector128.Create(PRIME32_1);
        Vector128<ulong> data = Sse2.Xor(acc, Sse2.ShiftRightLogical(acc, 47));
        Vector128<uint> dataKey = Sse2.Xor(data, Unsafe.ReadUnaligned<Vector128<ulong>>(secret)).AsUInt32();
        Vector128<ulong> productLow = Sse2.Multiply(dataKey, prime32);
        Vector128<ulong> productHigh = Sse2.Multiply(Sse2.ShiftRightLogical(dataKey.AsUInt64(), 32).AsUInt32(), prime32);
        return Sse2.Add(productLow, Sse2.ShiftLeftLogical(productHigh, 32));
    }
#endif

#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_initCustomSecret_avx2(byte* customSecret, ulong seed64)
    {
        Vector256<ulong> seed = Vector256.Create(seed64, 0UL - seed64, seed64, 0UL - seed64);

        fixed (byte* secret = &kSecret[0])
        {
            for (int i = 0; i < SECRET_DEFAULT_SIZE / 32; i++)
            {
                Vector256<ulong> src = Unsafe.ReadUnaligned<Vector256<ulong>>((ulong*)secret + (i * 4));
                Unsafe.WriteUnaligned((ulong*)customSecret + (i * 4), Avx2.Add(src, seed));
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_initCustomSecret_sse2(byte* customSecret, ulong seed64)
    {
        Vector128<ulong> seed = Vector128.Create(seed64, 0UL - seed64);

        fixed (byte* secret = &kSecret[0])
        {
            for (int i = 0; i < SECRET_DEFAULT_SIZE / 16; i++)
            {
                Vector128<ulong> src = Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)secret + (i * 2));
                Unsafe.WriteUnaligned((ulong*)customSecret + (i * 2), Sse2.Add(src, seed));
            }
        }
    }
#endif
}