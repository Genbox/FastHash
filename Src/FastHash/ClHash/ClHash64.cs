#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static Genbox.FastHash.ClHash.ClHashConstants;
using static Genbox.FastHash.ClHash.ClHashShared;

namespace Genbox.FastHash.ClHash;

public static class ClHash64
{
    private static readonly ulong[] _defaultKey = CreateKey(DefaultSeed1, DefaultSeed2);

    public static bool IsSupported => ClHash64Unsafe.IsSupported;

    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, _defaultKey);

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed1, ulong seed2)
    {
        Span<ulong> key = stackalloc ulong[Random64BitWordsNeeded];
        CreateKey(seed1, seed2, key);
        return ComputeHash(data, key);
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ReadOnlySpan<ulong> key)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("CLHash requires PCLMULQDQ, SSE2, and SSSE3 intrinsics.");

        ValidateKey(key);
        return ComputeHashCore(data, key);
    }

    internal static void ValidateKey(ReadOnlySpan<ulong> key)
    {
        if (key.Length != Random64BitWordsNeeded)
            throw new ArgumentException($"CLHash keys must contain exactly {Random64BitWordsNeeded} 64-bit words.", nameof(key));
    }

    private static ulong ComputeHashCore(ReadOnlySpan<byte> data, ReadOnlySpan<ulong> key)
    {
        int lengthBytes = data.Length;

        Vector128<byte> polyValue = Load128(key, WordsPerBlock);
        polyValue = Sse2.And(polyValue.AsUInt32(), Vector128.Create(0xffffffffU, 0xffffffffU, 0xffffffffU, 0x3fffffffU)).AsByte();

        int fullWords = lengthBytes / sizeof(ulong);
        int wordsIncludingPartial = (lengthBytes + sizeof(ulong) - 1) / sizeof(ulong);

        if (WordsPerBlock < wordsIncludingPartial)
        {
            Vector128<byte> acc = ClMulHalfScalarProductWithoutReduction(key, data, 0, WordsPerBlock);
            int t = WordsPerBlock;

            for (; t + WordsPerBlock <= fullWords; t += WordsPerBlock)
            {
                acc = Mul128By128To128LazyMod127(polyValue, acc);
                Vector128<byte> h1 = ClMulHalfScalarProductWithoutReduction(key, data, t * sizeof(ulong), WordsPerBlock);
                acc = Sse2.Xor(acc, h1);
            }

            int remain = fullWords - t;

            if (remain != 0)
            {
                acc = Mul128By128To128LazyMod127(polyValue, acc);

                Vector128<byte> h1;
                if ((lengthBytes % sizeof(ulong)) == 0)
                {
                    h1 = ClMulHalfScalarProductWithTailWithoutReduction(key, data, t * sizeof(ulong), remain);
                }
                else
                {
                    ulong lastWord = CreateLastWord(data, fullWords * sizeof(ulong), lengthBytes);
                    h1 = ClMulHalfScalarProductWithTailWithoutReductionWithExtraWord(key, data, t * sizeof(ulong), remain, lastWord);
                }

                acc = Sse2.Xor(acc, h1);
            }
            else if ((lengthBytes % sizeof(ulong)) != 0)
            {
                acc = Mul128By128To128LazyMod127(polyValue, acc);
                ulong lastWord = CreateLastWord(data, fullWords * sizeof(ulong), lengthBytes);
                Vector128<byte> h1 = ClMulHalfScalarProductOnlyExtraWord(key, lastWord);
                acc = Sse2.Xor(acc, h1);
            }

            Vector128<byte> finalKey = Load128(key, WordsPerBlock + 2);
            ulong keyLength = key[WordsPerBlock + 4];
            return Simple128To64HashWithLength(acc, finalKey, keyLength, (ulong)lengthBytes);
        }

        {
            Vector128<byte> acc;
            if ((lengthBytes % sizeof(ulong)) == 0)
            {
                acc = ClMulHalfScalarProductWithTailWithoutReduction(key, data, 0, fullWords);
            }
            else
            {
                ulong lastWord = CreateLastWord(data, fullWords * sizeof(ulong), lengthBytes);
                acc = ClMulHalfScalarProductWithTailWithoutReductionWithExtraWord(key, data, 0, fullWords, lastWord);
            }

            ulong keyLength = key[WordsPerBlock + 4];
            acc = Sse2.Xor(acc, LazyLengthHash(keyLength, (ulong)lengthBytes));
            return PrecompReduction64(acc);
        }
    }


    private static Vector128<byte> ClMulHalfScalarProductWithoutReduction(ReadOnlySpan<ulong> randomSource, ReadOnlySpan<byte> data, int dataOffset, int lengthWords)
    {
        int end = dataOffset + (lengthWords * sizeof(ulong));
        Vector128<byte> acc = Vector128<byte>.Zero;
        int keyOffset = 0;

        for (; dataOffset + (3 * sizeof(ulong)) < end; keyOffset += 4, dataOffset += 4 * sizeof(ulong))
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource, keyOffset), Load128(data, dataOffset));
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);

            Vector128<byte> add2 = Sse2.Xor(Load128(randomSource, keyOffset + 2), Load128(data, dataOffset + (2 * sizeof(ulong))));
            acc = Sse2.Xor(ClMul(add2, add2, 0x10), acc);
        }

        return acc;
    }

    private static Vector128<byte> ClMulHalfScalarProductWithTailWithoutReduction(ReadOnlySpan<ulong> randomSource, ReadOnlySpan<byte> data, int dataOffset, int lengthWords)
    {
        int end = dataOffset + (lengthWords * sizeof(ulong));
        Vector128<byte> acc = Vector128<byte>.Zero;
        int keyOffset = 0;

        for (; dataOffset + (3 * sizeof(ulong)) < end; keyOffset += 4, dataOffset += 4 * sizeof(ulong))
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource, keyOffset), Load128(data, dataOffset));
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);

            Vector128<byte> add2 = Sse2.Xor(Load128(randomSource, keyOffset + 2), Load128(data, dataOffset + (2 * sizeof(ulong))));
            acc = Sse2.Xor(ClMul(add2, add2, 0x10), acc);
        }

        if (dataOffset + sizeof(ulong) < end)
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource, keyOffset), Load128(data, dataOffset));
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);
            keyOffset += 2;
            dataOffset += 2 * sizeof(ulong);
        }

        if (dataOffset < end)
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource, keyOffset), Vector128.Create(Read64(data, dataOffset), 0UL).AsByte());
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);
        }

        return acc;
    }

    private static Vector128<byte> ClMulHalfScalarProductWithTailWithoutReductionWithExtraWord(ReadOnlySpan<ulong> randomSource, ReadOnlySpan<byte> data, int dataOffset, int lengthWords, ulong extraWord)
    {
        int end = dataOffset + (lengthWords * sizeof(ulong));
        Vector128<byte> acc = Vector128<byte>.Zero;
        int keyOffset = 0;

        for (; dataOffset + (3 * sizeof(ulong)) < end; keyOffset += 4, dataOffset += 4 * sizeof(ulong))
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource, keyOffset), Load128(data, dataOffset));
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);

            Vector128<byte> add2 = Sse2.Xor(Load128(randomSource, keyOffset + 2), Load128(data, dataOffset + (2 * sizeof(ulong))));
            acc = Sse2.Xor(ClMul(add2, add2, 0x10), acc);
        }

        if (dataOffset + sizeof(ulong) < end)
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource, keyOffset), Load128(data, dataOffset));
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);
            keyOffset += 2;
            dataOffset += 2 * sizeof(ulong);
        }

        if (dataOffset < end)
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource, keyOffset), Vector128.Create(Read64(data, dataOffset), extraWord).AsByte());
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);
        }
        else
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource, keyOffset), Vector128.Create(extraWord, 0UL).AsByte());
            acc = Sse2.Xor(ClMul(add1, add1, 0x01), acc);
        }

        return acc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> ClMulHalfScalarProductOnlyExtraWord(ReadOnlySpan<ulong> randomSource, ulong extraWord)
    {
        Vector128<byte> add1 = Sse2.Xor(Load128(randomSource, 0), Vector128.Create(extraWord, 0UL).AsByte());
        return ClMul(add1, add1, 0x01);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong CreateLastWord(ReadOnlySpan<byte> data, int offset, int lengthBytes)
    {
        int significantBytes = lengthBytes % sizeof(ulong);
        ulong lastWord = 0;

        for (int i = 0; i < significantBytes; i++)
            lastWord |= (ulong)data[offset + i] << (i * 8);

        return lastWord;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Load128(ReadOnlySpan<ulong> source, int offset)
    {
        ref ulong src = ref MemoryMarshal.GetReference(source);
        return Unsafe.ReadUnaligned<Vector128<ulong>>(ref Unsafe.As<ulong, byte>(ref Unsafe.Add(ref src, offset))).AsByte();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Load128(ReadOnlySpan<byte> source, int offset)
    {
        ref byte src = ref MemoryMarshal.GetReference(source);
        return Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref src, offset));
    }
}
#endif