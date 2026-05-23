#if NET8_0_OR_GREATER
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

    internal static unsafe ulong XXH3_mix16B(byte* input, byte* secret, ulong seed64)
    {
        ulong input_lo = Read64(input);
        ulong input_hi = Read64(input + 8);

        return XXH3_mul128_fold64(
            input_lo ^ (Read64(secret) + seed64),
            input_hi ^ (Read64(secret + 8) - seed64)
        );
    }

    internal unsafe delegate ulong XXH3_hashLong64_f_unsafe(byte* input, int len, ulong seed64, byte* secret, int secretLen);
    internal unsafe delegate UInt128 XXH3_hashLong128_f_unsafe(byte* input, int len, ulong seed64, byte* secret, int secretLen);

#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_hashLong_internal_loop_avx2(ulong* acc, byte* input, int len, byte* secret, int secretSize)
    {
        int nbStripesPerBlock = (secretSize - STRIPE_LEN) / SECRET_CONSUME_RATE;
        int block_len = STRIPE_LEN * nbStripesPerBlock;
        int nb_blocks = (len - 1) / block_len;

        for (int n = 0; n < nb_blocks; n++)
        {
            XXH3_accumulate_avx2(acc, input + (n * block_len), secret, nbStripesPerBlock);
            XXH3_scrambleAcc_avx2(acc, (secret + secretSize) - STRIPE_LEN);
        }

        int nbStripes = (len - 1 - (block_len * nb_blocks)) / STRIPE_LEN;
        XXH3_accumulate_avx2(acc, input + (nb_blocks * block_len), secret, nbStripes);

        byte* p = (input + len) - STRIPE_LEN;
        XXH3_accumulate_512_avx2(acc, p, (secret + secretSize) - STRIPE_LEN - SECRET_LASTACC_START);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_hashLong_internal_loop_sse2(ulong* acc, byte* input, int len, byte* secret, int secretSize)
    {
        int nbStripesPerBlock = (secretSize - STRIPE_LEN) / SECRET_CONSUME_RATE;
        int block_len = STRIPE_LEN * nbStripesPerBlock;
        int nb_blocks = (len - 1) / block_len;

        for (int n = 0; n < nb_blocks; n++)
        {
            XXH3_accumulate_sse2(acc, input + (n * block_len), secret, nbStripesPerBlock);
            XXH3_scrambleAcc_sse2(acc, (secret + secretSize) - STRIPE_LEN);
        }

        int nbStripes = (len - 1 - (block_len * nb_blocks)) / STRIPE_LEN;
        XXH3_accumulate_sse2(acc, input + (nb_blocks * block_len), secret, nbStripes);

        byte* p = (input + len) - STRIPE_LEN;
        XXH3_accumulate_512_sse2(acc, p, (secret + secretSize) - STRIPE_LEN - SECRET_LASTACC_START);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_accumulate_avx2(ulong* acc, byte* input, byte* secret, int nbStripes)
    {
        for (int n = 0; n < nbStripes; n++)
            XXH3_accumulate_512_avx2(acc, input + (n * STRIPE_LEN), secret + (n * SECRET_CONSUME_RATE));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_accumulate_sse2(ulong* acc, byte* input, byte* secret, int nbStripes)
    {
        for (int n = 0; n < nbStripes; n++)
            XXH3_accumulate_512_sse2(acc, input + (n * STRIPE_LEN), secret + (n * SECRET_CONSUME_RATE));
    }
#endif

#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_scrambleAcc_avx2(ulong* acc, byte* secret)
    {
        Vector256<uint> prime32 = Vector256.Create(PRIME32_1);

        Vector256<ulong> acc_vec0 = Unsafe.ReadUnaligned<Vector256<ulong>>(acc + 0);
        Vector256<ulong> acc_vec1 = Unsafe.ReadUnaligned<Vector256<ulong>>(acc + 4);

        Vector256<ulong> data_vec0 = Avx2.Xor(acc_vec0, Avx2.ShiftRightLogical(acc_vec0, 47));
        Vector256<ulong> data_vec1 = Avx2.Xor(acc_vec1, Avx2.ShiftRightLogical(acc_vec1, 47));

        Vector256<ulong> key_vec0 = Unsafe.ReadUnaligned<Vector256<ulong>>((ulong*)secret + 0);
        Vector256<ulong> key_vec1 = Unsafe.ReadUnaligned<Vector256<ulong>>((ulong*)secret + 4);

        Vector256<uint> data_key0 = Avx2.Xor(data_vec0, key_vec0).AsUInt32();
        Vector256<uint> data_key1 = Avx2.Xor(data_vec1, key_vec1).AsUInt32();

        Vector256<ulong> prod_lo0 = Avx2.Multiply(data_key0, prime32);
        Vector256<ulong> prod_lo1 = Avx2.Multiply(data_key1, prime32);
        Vector256<ulong> prod_hi0 = Avx2.Multiply(Avx2.ShiftRightLogical(data_key0.AsUInt64(), 32).AsUInt32(), prime32);
        Vector256<ulong> prod_hi1 = Avx2.Multiply(Avx2.ShiftRightLogical(data_key1.AsUInt64(), 32).AsUInt32(), prime32);

        Unsafe.WriteUnaligned(acc + 0, Avx2.Add(prod_lo0, Avx2.ShiftLeftLogical(prod_hi0, 32)));
        Unsafe.WriteUnaligned(acc + 4, Avx2.Add(prod_lo1, Avx2.ShiftLeftLogical(prod_hi1, 32)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_scrambleAcc_sse2(ulong* acc, byte* secret)
    {
        Vector128<uint> prime32 = Vector128.Create(PRIME32_1);

        Vector128<ulong> acc_vec0 = Unsafe.ReadUnaligned<Vector128<ulong>>(acc + 0);
        Vector128<ulong> acc_vec1 = Unsafe.ReadUnaligned<Vector128<ulong>>(acc + 2);
        Vector128<ulong> acc_vec2 = Unsafe.ReadUnaligned<Vector128<ulong>>(acc + 4);
        Vector128<ulong> acc_vec3 = Unsafe.ReadUnaligned<Vector128<ulong>>(acc + 6);

        Vector128<uint> data_key0 = Sse2.Xor(Sse2.Xor(acc_vec0, Sse2.ShiftRightLogical(acc_vec0, 47)), Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)secret + 0)).AsUInt32();
        Vector128<uint> data_key1 = Sse2.Xor(Sse2.Xor(acc_vec1, Sse2.ShiftRightLogical(acc_vec1, 47)), Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)secret + 2)).AsUInt32();
        Vector128<uint> data_key2 = Sse2.Xor(Sse2.Xor(acc_vec2, Sse2.ShiftRightLogical(acc_vec2, 47)), Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)secret + 4)).AsUInt32();
        Vector128<uint> data_key3 = Sse2.Xor(Sse2.Xor(acc_vec3, Sse2.ShiftRightLogical(acc_vec3, 47)), Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)secret + 6)).AsUInt32();

        Vector128<ulong> prod_lo0 = Sse2.Multiply(data_key0, prime32);
        Vector128<ulong> prod_lo1 = Sse2.Multiply(data_key1, prime32);
        Vector128<ulong> prod_lo2 = Sse2.Multiply(data_key2, prime32);
        Vector128<ulong> prod_lo3 = Sse2.Multiply(data_key3, prime32);
        Vector128<ulong> prod_hi0 = Sse2.Multiply(Sse2.ShiftRightLogical(data_key0.AsUInt64(), 32).AsUInt32(), prime32);
        Vector128<ulong> prod_hi1 = Sse2.Multiply(Sse2.ShiftRightLogical(data_key1.AsUInt64(), 32).AsUInt32(), prime32);
        Vector128<ulong> prod_hi2 = Sse2.Multiply(Sse2.ShiftRightLogical(data_key2.AsUInt64(), 32).AsUInt32(), prime32);
        Vector128<ulong> prod_hi3 = Sse2.Multiply(Sse2.ShiftRightLogical(data_key3.AsUInt64(), 32).AsUInt32(), prime32);

        Unsafe.WriteUnaligned(acc + 0, Sse2.Add(prod_lo0, Sse2.ShiftLeftLogical(prod_hi0, 32)));
        Unsafe.WriteUnaligned(acc + 2, Sse2.Add(prod_lo1, Sse2.ShiftLeftLogical(prod_hi1, 32)));
        Unsafe.WriteUnaligned(acc + 4, Sse2.Add(prod_lo2, Sse2.ShiftLeftLogical(prod_hi2, 32)));
        Unsafe.WriteUnaligned(acc + 6, Sse2.Add(prod_lo3, Sse2.ShiftLeftLogical(prod_hi3, 32)));
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
#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_accumulate_512_avx2(ulong* acc, byte* input, byte* secret)
    {
        Vector256<ulong> acc_vec0 = Unsafe.ReadUnaligned<Vector256<ulong>>(acc + 0);
        Vector256<ulong> acc_vec1 = Unsafe.ReadUnaligned<Vector256<ulong>>(acc + 4);

        Vector256<uint> data_vec0 = Unsafe.ReadUnaligned<Vector256<ulong>>((ulong*)input + 0).AsUInt32();
        Vector256<uint> data_vec1 = Unsafe.ReadUnaligned<Vector256<ulong>>((ulong*)input + 4).AsUInt32();

        Vector256<uint> key_vec0 = Unsafe.ReadUnaligned<Vector256<ulong>>((ulong*)secret + 0).AsUInt32();
        Vector256<uint> key_vec1 = Unsafe.ReadUnaligned<Vector256<ulong>>((ulong*)secret + 4).AsUInt32();

        Vector256<uint> data_key0 = Avx2.Xor(data_vec0, key_vec0);
        Vector256<uint> data_key1 = Avx2.Xor(data_vec1, key_vec1);

        Vector256<uint> data_key_lo0 = Avx2.Shuffle(data_key0, MM_SHUFFLE_0_3_0_1);
        Vector256<uint> data_key_lo1 = Avx2.Shuffle(data_key1, MM_SHUFFLE_0_3_0_1);

        Vector256<ulong> product0 = Avx2.Multiply(data_key0, data_key_lo0);
        Vector256<ulong> product1 = Avx2.Multiply(data_key1, data_key_lo1);

        Vector256<ulong> data_swap0 = Avx2.Shuffle(data_vec0, MM_SHUFFLE_1_0_3_2).AsUInt64();
        Vector256<ulong> data_swap1 = Avx2.Shuffle(data_vec1, MM_SHUFFLE_1_0_3_2).AsUInt64();

        Vector256<ulong> sum0 = Avx2.Add(acc_vec0, data_swap0);
        Vector256<ulong> sum1 = Avx2.Add(acc_vec1, data_swap1);

        Vector256<ulong> result0 = Avx2.Add(product0, sum0);
        Vector256<ulong> result1 = Avx2.Add(product1, sum1);

        Unsafe.WriteUnaligned(acc + 0, result0);
        Unsafe.WriteUnaligned(acc + 4, result1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void XXH3_accumulate_512_sse2(ulong* acc, byte* input, byte* secret)
    {
        Vector128<ulong> acc_vec0 = Unsafe.ReadUnaligned<Vector128<ulong>>(acc + 0);
        Vector128<ulong> acc_vec1 = Unsafe.ReadUnaligned<Vector128<ulong>>(acc + 2);
        Vector128<ulong> acc_vec2 = Unsafe.ReadUnaligned<Vector128<ulong>>(acc + 4);
        Vector128<ulong> acc_vec3 = Unsafe.ReadUnaligned<Vector128<ulong>>(acc + 6);

        Vector128<uint> data_vec0 = Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)input + 0).AsUInt32();
        Vector128<uint> data_vec1 = Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)input + 2).AsUInt32();
        Vector128<uint> data_vec2 = Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)input + 4).AsUInt32();
        Vector128<uint> data_vec3 = Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)input + 6).AsUInt32();

        Vector128<uint> key_vec0 = Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)secret + 0).AsUInt32();
        Vector128<uint> key_vec1 = Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)secret + 2).AsUInt32();
        Vector128<uint> key_vec2 = Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)secret + 4).AsUInt32();
        Vector128<uint> key_vec3 = Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)secret + 6).AsUInt32();

        Vector128<uint> data_key0 = Sse2.Xor(data_vec0, key_vec0);
        Vector128<uint> data_key1 = Sse2.Xor(data_vec1, key_vec1);
        Vector128<uint> data_key2 = Sse2.Xor(data_vec2, key_vec2);
        Vector128<uint> data_key3 = Sse2.Xor(data_vec3, key_vec3);

        Vector128<uint> data_key_lo0 = Sse2.Shuffle(data_key0, MM_SHUFFLE_0_3_0_1);
        Vector128<uint> data_key_lo1 = Sse2.Shuffle(data_key1, MM_SHUFFLE_0_3_0_1);
        Vector128<uint> data_key_lo2 = Sse2.Shuffle(data_key2, MM_SHUFFLE_0_3_0_1);
        Vector128<uint> data_key_lo3 = Sse2.Shuffle(data_key3, MM_SHUFFLE_0_3_0_1);

        Vector128<ulong> product0 = Sse2.Multiply(data_key0, data_key_lo0);
        Vector128<ulong> product1 = Sse2.Multiply(data_key1, data_key_lo1);
        Vector128<ulong> product2 = Sse2.Multiply(data_key2, data_key_lo2);
        Vector128<ulong> product3 = Sse2.Multiply(data_key3, data_key_lo3);

        Vector128<ulong> data_swap0 = Sse2.Shuffle(data_vec0, MM_SHUFFLE_1_0_3_2).AsUInt64();
        Vector128<ulong> data_swap1 = Sse2.Shuffle(data_vec1, MM_SHUFFLE_1_0_3_2).AsUInt64();
        Vector128<ulong> data_swap2 = Sse2.Shuffle(data_vec2, MM_SHUFFLE_1_0_3_2).AsUInt64();
        Vector128<ulong> data_swap3 = Sse2.Shuffle(data_vec3, MM_SHUFFLE_1_0_3_2).AsUInt64();

        Vector128<ulong> sum0 = Sse2.Add(acc_vec0, data_swap0);
        Vector128<ulong> sum1 = Sse2.Add(acc_vec1, data_swap1);
        Vector128<ulong> sum2 = Sse2.Add(acc_vec2, data_swap2);
        Vector128<ulong> sum3 = Sse2.Add(acc_vec3, data_swap3);

        Vector128<ulong> result0 = Sse2.Add(product0, sum0);
        Vector128<ulong> result1 = Sse2.Add(product1, sum1);
        Vector128<ulong> result2 = Sse2.Add(product2, sum2);
        Vector128<ulong> result3 = Sse2.Add(product3, sum3);

        Unsafe.WriteUnaligned(acc + 0, result0);
        Unsafe.WriteUnaligned(acc + 2, result1);
        Unsafe.WriteUnaligned(acc + 4, result2);
        Unsafe.WriteUnaligned(acc + 6, result3);
    }
#endif
}