using System.Runtime.CompilerServices;
using static Genbox.FastHash.XxHash.XxHashConstants;

namespace Genbox.FastHash.XxHash;

internal static class XxHashShared
{
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

    internal static ulong XXH3_mix16B(ReadOnlySpan<byte> input, int offset, ReadOnlySpan<byte> secret, int secretOffset, ulong seed64)
    {
        ulong input_lo = Read64(input, offset);
        ulong input_hi = Read64(input, offset + 8);

        return XXH3_mul128_fold64(
            input_lo ^ (Read64(secret, secretOffset) + seed64),
            input_hi ^ (Read64(secret, secretOffset + 8) - seed64)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void XXH3_hashLong_internal_loop(Span<ulong> acc, ReadOnlySpan<byte> input, int len, ReadOnlySpan<byte> secret, int secretSize, XXH3_f_accumulate_512 f_acc512, XXH3_f_scrambleAcc f_scramble)
    {
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
    private static void XXH3_accumulate(Span<ulong> acc, ReadOnlySpan<byte> input, ReadOnlySpan<byte> secret, int nbStripes, XXH3_f_accumulate_512 f_acc512)
    {
        for (int n = 0; n < nbStripes; n++)
        {
            ReadOnlySpan<byte> inp = input.Slice(n * STRIPE_LEN);
            f_acc512(acc, inp, secret.Slice(n * SECRET_CONSUME_RATE));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XXH3_accumulate_scalar(Span<ulong> acc, ReadOnlySpan<byte> input, int inputOffset, ReadOnlySpan<byte> secret, int secretOffset, int nbStripes)
    {
        for (int n = 0; n < nbStripes; n++)
            XXH3_accumulate_512_scalar(acc, input, inputOffset + (n * STRIPE_LEN), secret, secretOffset + (n * SECRET_CONSUME_RATE));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void XXH3_accumulate_512_scalar(Span<ulong> acc, ReadOnlySpan<byte> input, ReadOnlySpan<byte> secret)
    {
        XXH3_accumulate_512_scalar(acc, input, 0, secret, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XXH3_accumulate_512_scalar(Span<ulong> acc, ReadOnlySpan<byte> input, int inputOffset, ReadOnlySpan<byte> secret, int secretOffset)
    {
        for (int i = 0; i < ACC_NB; i++)
            XXH3_scalarRound(acc, input, inputOffset, secret, secretOffset, i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XXH3_scalarRound(Span<ulong> acc, ReadOnlySpan<byte> input, ReadOnlySpan<byte> secret, int lane)
    {
        XXH3_scalarRound(acc, input, 0, secret, 0, lane);
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
    internal static void XXH3_scrambleAcc_scalar(Span<ulong> acc, ReadOnlySpan<byte> secret)
    {
        XXH3_scrambleAcc_scalar(acc, secret, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XXH3_scrambleAcc_scalar(Span<ulong> acc, ReadOnlySpan<byte> secret, int secretOffset)
    {
        for (int i = 0; i < ACC_NB; i++)
            XXH3_scalarScrambleRound(acc, secret, secretOffset, i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XXH3_scalarScrambleRound(Span<ulong> acc, ReadOnlySpan<byte> secret, int lane)
    {
        XXH3_scalarScrambleRound(acc, secret, 0, lane);
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

    internal delegate ulong XXH3_hashLong64_f(ReadOnlySpan<byte> input, int len, ulong seed64, ReadOnlySpan<byte> secret, int secretLen);
    internal delegate UInt128 XXH3_hashLong128_f(ReadOnlySpan<byte> input, int len, ulong seed64, ReadOnlySpan<byte> secret, int secretLen);
    internal delegate void XXH3_f_scrambleAcc(Span<ulong> acc, ReadOnlySpan<byte> secret);
    internal delegate void XXH3_f_initCustomSecret(Span<byte> customSecret, ulong seed);
    internal delegate void XXH3_f_accumulate_512(Span<ulong> acc, ReadOnlySpan<byte> input, ReadOnlySpan<byte> secret);
}