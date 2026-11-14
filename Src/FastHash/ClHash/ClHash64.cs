#if NET8_0_OR_GREATER

// C# port of simdhash/clhash 1.0.0 (fd0331c) by Daniel Lemire and Owen Kaser.
// Distributed under the Apache License 2.0; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static Genbox.FastHash.ClHash.ClHashConstants;

namespace Genbox.FastHash.ClHash;

/// <summary>Provides 64-bit CLHash functions.</summary>
public static class ClHash64
{
    private static readonly ulong[] _defaultKey = CreateKey(DefaultSeed1, DefaultSeed2);

    /// <summary>Gets whether the required CPU intrinsics are available.</summary>
    public static bool IsSupported => Pclmulqdq.IsSupported && Sse2.IsSupported && Ssse3.IsSupported;

    /// <summary>Creates a CLHash key from two seeds.</summary>
    /// <param name="seed1">The first key seed.</param>
    /// <param name="seed2">The second key seed.</param>
    /// <returns>A CLHash key.</returns>
    public static ulong[] CreateKey(ulong seed1, ulong seed2)
    {
        ValidateSeeds(seed1, seed2);

        ulong[] key = new ulong[Random64BitWordsNeeded];
        CreateKeyCore(seed1, seed2, key);
        return key;
    }

    /// <summary>Computes the hash of data with the default key.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, _defaultKey);

    /// <summary>Computes the hash of data using a key derived from two seeds.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed1">The first key seed.</param>
    /// <param name="seed2">The second key seed.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed1, ulong seed2)
    {
        Span<ulong> key = stackalloc ulong[Random64BitWordsNeeded];
        ClHash64.CreateKey(seed1, seed2, key);
        return ComputeHash(data, key);
    }

    /// <summary>Computes the hash of data using a CLHash key.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="key">The CLHash key.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentException"><paramref name="key" /> is invalid.</exception>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
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

        ValidatePolynomial(key[WordsPerBlock], key[WordsPerBlock + 1], nameof(key));
    }

    internal static void ValidatePolynomial(ulong low, ulong high, string paramName)
    {
        if (IsInvalidPolynomial(low, high))
            throw new ArgumentException("CLHash keys must have a non-zero, non-degenerate polynomial multiplier.", paramName);
    }

    private static ulong ComputeHashCore(ReadOnlySpan<byte> data, ReadOnlySpan<ulong> key)
    {
        int lengthBytes = data.Length;

        Vector128<byte> polyValue = Load128(key, WordsPerBlock);
        polyValue = Sse2.And(polyValue.AsUInt32(), Vector128.Create(0xffffffffU, 0xffffffffU, 0xffffffffU, 0x3fffffffU)).AsByte();

        int fullWords = lengthBytes / sizeof(ulong);
        int wordsIncludingPartial = ((lengthBytes + sizeof(ulong)) - 1) / sizeof(ulong);

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

                if (lengthBytes % sizeof(ulong) == 0)
                    h1 = ClMulHalfScalarProductWithTailWithoutReduction(key, data, t * sizeof(ulong), remain);
                else
                {
                    ulong lastWord = CreateLastWord(data, fullWords * sizeof(ulong), lengthBytes);
                    h1 = ClMulHalfScalarProductWithTailWithoutReductionWithExtraWord(key, data, t * sizeof(ulong), remain, lastWord);
                }

                acc = Sse2.Xor(acc, h1);
            }
            else if (lengthBytes % sizeof(ulong) != 0)
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

            if (lengthBytes % sizeof(ulong) == 0)
                acc = ClMulHalfScalarProductWithTailWithoutReduction(key, data, 0, fullWords);
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
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
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
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource, keyOffset), Vector128.Create(Read64(ref dataReference, dataOffset), 0UL).AsByte());
            acc = Sse2.Xor(ClMul(add1, add1, 0x10), acc);
        }

        return acc;
    }

    private static Vector128<byte> ClMulHalfScalarProductWithTailWithoutReductionWithExtraWord(ReadOnlySpan<ulong> randomSource, ReadOnlySpan<byte> data, int dataOffset, int lengthWords, ulong extraWord)
    {
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
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
            Vector128<byte> add1 = Sse2.Xor(Load128(randomSource, keyOffset), Vector128.Create(Read64(ref dataReference, dataOffset), extraWord).AsByte());
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
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        int significantBytes = lengthBytes % sizeof(ulong);
        ulong lastWord = 0;

        for (int i = 0; i < significantBytes; i++)
            lastWord |= (ulong)Unsafe.Add(ref dataReference, offset + i) << (i * 8);

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

    internal static void CreateKey(ulong seed1, ulong seed2, Span<ulong> key)
    {
        ValidateSeeds(seed1, seed2);

        if (key.Length != Random64BitWordsNeeded)
            throw new ArgumentException($"CLHash keys must contain exactly {Random64BitWordsNeeded} 64-bit words.", nameof(key));

        CreateKeyCore(seed1, seed2, key);
    }

    private static void ValidateSeeds(ulong seed1, ulong seed2)
    {
        if (seed1 == 0)
            throw new ArgumentOutOfRangeException(nameof(seed1), "CLHash seeds must be non-zero.");

        if (seed2 == 0)
            throw new ArgumentOutOfRangeException(nameof(seed2), "CLHash seeds must be non-zero.");
    }

    private static void CreateKeyCore(ulong seed1, ulong seed2, Span<ulong> key)
    {
        ulong part1 = seed1;
        ulong part2 = seed2;

        for (int i = 0; i < key.Length; i++)
            key[i] = XorShift128Plus(ref part1, ref part2);

        while (IsInvalidPolynomial(key[WordsPerBlock], key[WordsPerBlock + 1]))
        {
            key[WordsPerBlock] = XorShift128Plus(ref part1, ref part2);
            key[WordsPerBlock + 1] = XorShift128Plus(ref part1, ref part2);
        }
    }

    internal static bool IsInvalidPolynomial(ulong low, ulong high) =>
        low == 0 && ((high & 0x3fff_ffff_ffff_ffffUL) == 0 || high == 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong XorShift128Plus(ref ulong part1, ref ulong part2)
    {
        ulong s1 = part1;
        ulong s0 = part2;
        part1 = s0;
        s1 ^= s1 << 23;
        part2 = s1 ^ s0 ^ (s1 >> 18) ^ (s0 >> 5);
        return part2 + s0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> LeftShift1(Vector128<byte> value) => LeftShift(value, 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> LeftShift2(Vector128<byte> value) => LeftShift(value, 2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Performance", "CA1857:Use a constant for the best performance", Justification = "Inlining callers supply constant shift counts 1 and 2.")]
    internal static Vector128<byte> LeftShift(Vector128<byte> value, [ConstantExpected] byte count)
    {
        Vector128<ulong> v = value.AsUInt64();
        Vector128<byte> u64Shift = Sse2.ShiftLeftLogical(v, count).AsByte();
        Vector128<byte> topBits = Sse2.ShiftLeftLogical128BitLane(Sse2.ShiftRightLogical(v, (byte)(64 - count)).AsByte(), sizeof(ulong));
        return Sse2.Or(u64Shift, topBits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> LazyMod127(Vector128<byte> low, Vector128<byte> high) => Sse2.Xor(Sse2.Xor(low, LeftShift1(high)), LeftShift2(high));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> Mul128By128To128LazyMod127(Vector128<byte> a, Vector128<byte> b)
    {
        Vector128<byte> mix1 = ClMul(a, b, 0x01);
        Vector128<byte> mix2 = ClMul(a, b, 0x10);
        Vector128<byte> low = ClMul(a, b, 0x00);
        Vector128<byte> high = ClMul(a, b, 0x11);
        Vector128<byte> mix = Sse2.Xor(mix1, mix2);
        mix1 = Sse2.ShiftLeftLogical128BitLane(mix, sizeof(ulong));
        mix2 = Sse2.ShiftRightLogical128BitLane(mix, sizeof(ulong));
        low = Sse2.Xor(low, mix1);
        high = Sse2.Xor(high, mix2);
        return LazyMod127(low, high);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> LazyLengthHash(ulong keyLength, ulong length)
    {
        Vector128<byte> lengthVector = Vector128.Create(length, keyLength).AsByte();
        return ClMul(lengthVector, lengthVector, 0x10);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> PrecompReduction64Vector(Vector128<byte> value)
    {
        Vector128<byte> c = Vector128.Create(0x1bUL, 0UL).AsByte();
        Vector128<byte> q2 = ClMul(value, c, 0x01);
        Vector128<byte> table = Vector128.Create(
            0, 27, 54, 45,
            108, 119, 90, 65,
            216, 195, 238, 245,
            180, 175, 130, 153);
        Vector128<byte> q3 = Ssse3.Shuffle(table, Sse2.ShiftRightLogical128BitLane(q2, sizeof(ulong)));
        Vector128<byte> q4 = Sse2.Xor(q2, value);
        return Sse2.Xor(q3, q4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong PrecompReduction64(Vector128<byte> value) => PrecompReduction64Vector(value).AsUInt64().GetElement(0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Simple128To64HashWithLength(Vector128<byte> value, Vector128<byte> key, ulong keyLength, ulong length)
    {
        Vector128<byte> add = Sse2.Xor(value, key);
        Vector128<byte> product = ClMul(add, add, 0x10);
        Vector128<byte> total = Sse2.Xor(product, LazyLengthHash(keyLength, length));
        return PrecompReduction64(total);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> ClMul(Vector128<byte> left, Vector128<byte> right, [ConstantExpected] byte control) => Pclmulqdq.CarrylessMultiply(left.AsInt64(), right.AsInt64(), control).AsByte();
}
#endif