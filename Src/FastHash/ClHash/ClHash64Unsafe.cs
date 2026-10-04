#if NET8_0_OR_GREATER
// C# port of simdhash/clhash 1.0.0 (fd0331c) by Daniel Lemire and Owen Kaser.
// Distributed under the Apache License 2.0; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static Genbox.FastHash.ClHash.ClHashConstants;
using static Genbox.FastHash.ClHash.ClHashShared;

namespace Genbox.FastHash.ClHash;

/// <summary>Provides pointer-based 64-bit CLHash functions.</summary>
public static class ClHash64Unsafe
{
    private static readonly ulong[] _defaultKey = CreateKey(DefaultSeed1, DefaultSeed2);

    /// <summary>Gets whether the required CPU intrinsics are available.</summary>
    public static bool IsSupported => Pclmulqdq.IsSupported && Sse2.IsSupported && Ssse3.IsSupported;

    /// <summary>Computes the hash of a memory region with the default key.</summary>
    /// <param name="data">A pointer to the data to hash.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        fixed (ulong* key = _defaultKey)
            return ComputeHash(data, length, key);
    }

    /// <summary>Computes the hash of a memory region using a key derived from two seeds.</summary>
    /// <param name="data">A pointer to the data to hash.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed1">The first key seed.</param>
    /// <param name="seed2">The second key seed.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed1, ulong seed2)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        Span<ulong> key = stackalloc ulong[Random64BitWordsNeeded];
        CreateKey(seed1, seed2, key);

        fixed (ulong* keyPtr = key)
            return ComputeHash(data, length, keyPtr);
    }

    /// <summary>Computes the hash of a memory region using a CLHash key.</summary>
    /// <param name="data">A pointer to the data to hash.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="key">The CLHash key.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentException"><paramref name="key" /> is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length, ReadOnlySpan<ulong> key)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ClHash64.ValidateKey(key);

        fixed (ulong* keyPtr = key)
            return ComputeHash(data, length, keyPtr);
    }

    /// <summary>Computes the hash of a memory region using a CLHash key pointer.</summary>
    /// <param name="data">A pointer to the data to hash.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="key">A pointer to the CLHash key.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length, ulong* key)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (!IsSupported)
            throw new PlatformNotSupportedException("CLHash requires PCLMULQDQ, SSE2, and SSSE3 intrinsics.");

        ClHash64.ValidatePolynomial(key[WordsPerBlock], key[WordsPerBlock + 1], nameof(key));

        return ComputeHashCore(data, length, key);
    }

    private static unsafe ulong ComputeHashCore(byte* data, int lengthBytes, ulong* key)
    {
        Vector128<byte> polyValue = Load128(key + WordsPerBlock);
        polyValue = Sse2.And(polyValue.AsUInt32(), Vector128.Create(0xffffffffU, 0xffffffffU, 0xffffffffU, 0x3fffffffU)).AsByte();

        int fullWords = lengthBytes / sizeof(ulong);
        int wordsIncludingPartial = ((lengthBytes + sizeof(ulong)) - 1) / sizeof(ulong);

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

                if (lengthBytes % sizeof(ulong) == 0)
                    h1 = ClMulHalfScalarProductWithTailWithoutReduction(key, data + (t * sizeof(ulong)), remain);
                else
                {
                    ulong lastWord = CreateLastWord(lengthBytes, data + (fullWords * sizeof(ulong)));
                    h1 = ClMulHalfScalarProductWithTailWithoutReductionWithExtraWord(key, data + (t * sizeof(ulong)), remain, lastWord);
                }

                acc = Sse2.Xor(acc, h1);
            }
            else if (lengthBytes % sizeof(ulong) != 0)
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

            if (lengthBytes % sizeof(ulong) == 0)
                acc = ClMulHalfScalarProductWithTailWithoutReduction(key, data, fullWords);
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