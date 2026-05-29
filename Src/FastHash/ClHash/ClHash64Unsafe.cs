#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static Genbox.FastHash.ClHash.ClHashConstants;
using static Genbox.FastHash.ClHash.ClHashShared;

namespace Genbox.FastHash.ClHash;

public static class ClHash64Unsafe
{
    private static readonly ulong[] _defaultKey = CreateKey(DefaultSeed1, DefaultSeed2);

    public static bool IsSupported => Pclmulqdq.IsSupported && Sse2.IsSupported && Ssse3.IsSupported;

    public static unsafe ulong ComputeHash(byte* data, int length)
    {
        fixed (ulong* key = _defaultKey)
            return ComputeHash(data, length, key);
    }

    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed1, ulong seed2)
    {
        Span<ulong> key = stackalloc ulong[Random64BitWordsNeeded];
        CreateKey(seed1, seed2, key);

        fixed (ulong* keyPtr = key)
            return ComputeHash(data, length, keyPtr);
    }

    public static unsafe ulong ComputeHash(byte* data, int length, ReadOnlySpan<ulong> key)
    {
        ClHash64.ValidateKey(key);

        fixed (ulong* keyPtr = key)
            return ComputeHash(data, length, keyPtr);
    }

    public static unsafe ulong ComputeHash(byte* data, int length, ulong* key)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("CLHash requires PCLMULQDQ, SSE2, and SSSE3 intrinsics.");

        ArgumentOutOfRangeException.ThrowIfNegative(length);

        return ComputeHashCore(data, length, key);
    }

    private static unsafe ulong ComputeHashCore(byte* data, int lengthBytes, ulong* key)
    {
        Vector128<byte> polyValue = Load128(key + WordsPerBlock);
        polyValue = Sse2.And(polyValue.AsUInt32(), Vector128.Create(0xffffffffU, 0xffffffffU, 0xffffffffU, 0x3fffffffU)).AsByte();

        int fullWords = lengthBytes / sizeof(ulong);
        int wordsIncludingPartial = (lengthBytes + sizeof(ulong) - 1) / sizeof(ulong);

        if (WordsPerBlock < wordsIncludingPartial)
        {
            Vector128<byte> acc = ClMulHalfScalarProductWithoutReduction(key, data, WordsPerBlock);
            int t = WordsPerBlock;

            for (; t + WordsPerBlock <= fullWords; t += WordsPerBlock)
            {
                acc = Mul128By128To128LazyMod127(polyValue, acc);
                Vector128<byte> h1 = ClMulHalfScalarProductWithoutReduction(key, data + (t * sizeof(ulong)), WordsPerBlock);
                acc = Sse2.Xor(acc, h1);
            }

            int remain = fullWords - t;

            if (remain != 0)
            {
                acc = Mul128By128To128LazyMod127(polyValue, acc);

                Vector128<byte> h1;
                if ((lengthBytes % sizeof(ulong)) == 0)
                {
                    h1 = ClMulHalfScalarProductWithTailWithoutReduction(key, data + (t * sizeof(ulong)), remain);
                }
                else
                {
                    ulong lastWord = CreateLastWord(lengthBytes, data + (fullWords * sizeof(ulong)));
                    h1 = ClMulHalfScalarProductWithTailWithoutReductionWithExtraWord(key, data + (t * sizeof(ulong)), remain, lastWord);
                }

                acc = Sse2.Xor(acc, h1);
            }
            else if ((lengthBytes % sizeof(ulong)) != 0)
            {
                acc = Mul128By128To128LazyMod127(polyValue, acc);
                ulong lastWord = CreateLastWord(lengthBytes, data + (fullWords * sizeof(ulong)));
                Vector128<byte> h1 = ClMulHalfScalarProductOnlyExtraWord(key, lastWord);
                acc = Sse2.Xor(acc, h1);
            }

            Vector128<byte> finalKey = Load128(key + WordsPerBlock + 2);
            ulong keyLength = key[WordsPerBlock + 4];
            return Simple128To64HashWithLength(acc, finalKey, keyLength, (ulong)lengthBytes);
        }

        {
            Vector128<byte> acc;
            if ((lengthBytes % sizeof(ulong)) == 0)
            {
                acc = ClMulHalfScalarProductWithTailWithoutReduction(key, data, fullWords);
            }
            else
            {
                ulong lastWord = CreateLastWord(lengthBytes, data + (fullWords * sizeof(ulong)));
                acc = ClMulHalfScalarProductWithTailWithoutReductionWithExtraWord(key, data, fullWords, lastWord);
            }

            ulong keyLength = key[WordsPerBlock + 4];
            acc = Sse2.Xor(acc, LazyLengthHash(keyLength, (ulong)lengthBytes));
            return PrecompReduction64(acc);
        }
    }


    private static unsafe Vector128<byte> ClMulHalfScalarProductWithoutReduction(ulong* randomSource, byte* data, int lengthWords)
    {
        byte* end = data + (lengthWords * sizeof(ulong));
        Vector128<byte> acc = Vector128<byte>.Zero;

        for (; data + (3 * sizeof(ulong)) < end; randomSource += 4, data += 4 * sizeof(ulong))
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource), Load128(data));
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);

            Vector128<byte> add2 = Sse2.Xor(Load128(randomSource + 2), Load128(data + (2 * sizeof(ulong))));
            acc = Sse2.Xor(ClMul(add2, add2, 0x10), acc);
        }

        return acc;
    }

    private static unsafe Vector128<byte> ClMulHalfScalarProductWithTailWithoutReduction(ulong* randomSource, byte* data, int lengthWords)
    {
        byte* end = data + (lengthWords * sizeof(ulong));
        Vector128<byte> acc = Vector128<byte>.Zero;

        for (; data + (3 * sizeof(ulong)) < end; randomSource += 4, data += 4 * sizeof(ulong))
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource), Load128(data));
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);

            Vector128<byte> add2 = Sse2.Xor(Load128(randomSource + 2), Load128(data + (2 * sizeof(ulong))));
            acc = Sse2.Xor(ClMul(add2, add2, 0x10), acc);
        }

        if (data + sizeof(ulong) < end)
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource), Load128(data));
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);
            randomSource += 2;
            data += 2 * sizeof(ulong);
        }

        if (data < end)
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource), Vector128.Create(Read64(data), 0UL).AsByte());
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);
        }

        return acc;
    }

    private static unsafe Vector128<byte> ClMulHalfScalarProductWithTailWithoutReductionWithExtraWord(ulong* randomSource, byte* data, int lengthWords, ulong extraWord)
    {
        byte* end = data + (lengthWords * sizeof(ulong));
        Vector128<byte> acc = Vector128<byte>.Zero;

        for (; data + (3 * sizeof(ulong)) < end; randomSource += 4, data += 4 * sizeof(ulong))
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource), Load128(data));
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);

            Vector128<byte> add2 = Sse2.Xor(Load128(randomSource + 2), Load128(data + (2 * sizeof(ulong))));
            acc = Sse2.Xor(ClMul(add2, add2, 0x10), acc);
        }

        if (data + sizeof(ulong) < end)
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource), Load128(data));
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);
            randomSource += 2;
            data += 2 * sizeof(ulong);
        }

        if (data < end)
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource), Vector128.Create(Read64(data), extraWord).AsByte());
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);
        }
        else
        {
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource), Vector128.Create(extraWord, 0UL).AsByte());
            acc = Sse2.Xor(ClMul(add1, add1, 0x01), acc);
        }

        return acc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Vector128<byte> ClMulHalfScalarProductOnlyExtraWord(ulong* randomSource, ulong extraWord)
    {
        Vector128<byte> add1 = Sse2.Xor(Load128(randomSource), Vector128.Create(extraWord, 0UL).AsByte());
        return ClMul(add1, add1, 0x01);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong CreateLastWord(int lengthBytes, byte* last)
    {
        int significantBytes = lengthBytes % sizeof(ulong);
        ulong lastWord = 0;

        for (int i = 0; i < significantBytes; i++)
            lastWord |= (ulong)last[i] << (i * 8);

        return lastWord;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Vector128<byte> Load128(ulong* ptr) => Unsafe.ReadUnaligned<Vector128<ulong>>(ptr).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Vector128<byte> Load128(byte* ptr) => Unsafe.ReadUnaligned<Vector128<byte>>(ptr);
}
#endif