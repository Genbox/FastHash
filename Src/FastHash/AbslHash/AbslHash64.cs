// C# port of the Abseil low-level hash from abseil/abseil-cpp (ddbc605). Copyright 2018 The Abseil Authors.
// Distributed under the Apache License 2.0; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using static Genbox.FastHash.AbslHash.AbslHashConstants;

#if NET8_0_OR_GREATER
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace Genbox.FastHash.AbslHash;

/// <summary>Provides 64-bit Abseil hash functions.</summary>
public static class AbslHash64
{
    /// <summary>Gets whether the SIMD implementation is available.</summary>
#if NET8_0_OR_GREATER
    public static bool IsSimdSupported => IsAesSupported;
#else
    public static bool IsSimdSupported => false;
#endif

    /// <summary>Computes the hash of data with the default seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    /// <summary>Computes the hash of data with a seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed) => CombineContiguous(seed, data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong CombineContiguous(ulong state, ReadOnlySpan<byte> data)
    {
        int len = data.Length;

        if (len <= 8)
            return CombineSmallContiguous(PrecombineLengthMix(state, len), data, len);

        if (len <= 16)
            return CombineContiguous9To16(PrecombineLengthMix(state, len), data, len);

        if (len <= 32)
            return CombineContiguous17To32(PrecombineLengthMix(state, len), data, len);

        return CombineLargeContiguous(state, data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong PrecombineLengthMix(ulong state, int len) => state ^ Read64(StaticRandomData, len);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong CombineSmallContiguous(ulong state, ReadOnlySpan<byte> data, int len)
    {
        ulong value;

        if (len >= 4)
            value = Read4To8(data, len);
        else if (len > 0)
            value = Read1To3(data, len);
        else
            value = 0x57;

        return CombineRaw(state, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong CombineContiguous9To16(ulong state, ReadOnlySpan<byte> data, int len) => Mix(state ^ Read64(data), Mul ^ Read64(data, len - 8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong CombineContiguous17To32(ulong state, ReadOnlySpan<byte> data, int len)
    {
        ulong m0 = Mix(Read64(data) ^ 0x13198a2e03707344UL, Read64(data, 8) ^ state);
        int tail16 = len - 16;
        ulong m1 = Mix(Read64(data, tail16) ^ 0x082efa98ec4e6c89UL, Read64(data, tail16 + 8) ^ state);
        return m0 ^ m1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Read4To8(ReadOnlySpan<byte> data, int len)
    {
        ulong mostSignificant = (ulong)Read32(data) << 32;
        ulong leastSignificant = Read32(data, len - 4);
        return mostSignificant | leastSignificant;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Read1To3(ReadOnlySpan<byte> data, int len)
    {
        uint mem0 = ((uint)data[0] << 16) | data[len - 1];
        uint mem1 = (uint)data[len / 2] << 8;
        return mem0 | mem1;
    }

    private static ulong CombineLargeContiguous(ulong state, ReadOnlySpan<byte> data)
    {
#if NET8_0_OR_GREATER
        if (IsAesSupported)
            return CombineLargeContiguousSimd(state, data);
#endif

        int len = data.Length;

        if (len <= PiecewiseChunkSize)
            return LowLevelHashLenGt32(state, data, len);

        while (len >= PiecewiseChunkSize)
        {
            state = LowLevelHashLenGt32(state, data[..PiecewiseChunkSize], PiecewiseChunkSize);
            data = data.Slice(PiecewiseChunkSize);
            len -= PiecewiseChunkSize;
        }

        return len == 0 ? state : CombineContiguous(state, data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong LowLevelHashLenGt32(ulong seed, ReadOnlySpan<byte> data, int len) => len > 64 ? LowLevelHashLenGt64(seed, data, len) : LowLevelHash33To64(seed, data, len);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Mix32Bytes(ReadOnlySpan<byte> data, int offset, ulong currentState)
    {
        ulong a = Read64(data, offset);
        ulong b = Read64(data, offset + 8);
        ulong c = Read64(data, offset + 16);
        ulong d = Read64(data, offset + 24);

        ulong cs0 = Mix(a ^ 0x13198a2e03707344UL, b ^ currentState);
        ulong cs1 = Mix(c ^ 0xa4093822299f31d0UL, d ^ currentState);
        return cs0 ^ cs1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong LowLevelHash33To64(ulong seed, ReadOnlySpan<byte> data, int len)
    {
        ulong currentState = seed ^ 0x243f6a8885a308d3UL ^ (uint)len;
        return Mix32Bytes(data, len - 32, Mix32Bytes(data, 0, currentState));
    }

    private static ulong LowLevelHashLenGt64(ulong seed, ReadOnlySpan<byte> data, int len)
    {
        ulong currentState = seed ^ 0x243f6a8885a308d3UL ^ (uint)len;
        int last32 = len - 32;
        ulong duplicatedState0 = currentState;
        ulong duplicatedState1 = currentState;
        ulong duplicatedState2 = currentState;
        int offset = 0;

        do
        {
            ulong a = Read64(data, offset);
            ulong b = Read64(data, offset + 8);
            ulong c = Read64(data, offset + 16);
            ulong d = Read64(data, offset + 24);
            ulong e = Read64(data, offset + 32);
            ulong f = Read64(data, offset + 40);
            ulong g = Read64(data, offset + 48);
            ulong h = Read64(data, offset + 56);

            currentState = Mix(a ^ 0x13198a2e03707344UL, b ^ currentState);
            duplicatedState0 = Mix(c ^ 0xa4093822299f31d0UL, d ^ duplicatedState0);
            duplicatedState1 = Mix(e ^ 0x082efa98ec4e6c89UL, f ^ duplicatedState1);
            duplicatedState2 = Mix(g ^ 0x452821e638d01377UL, h ^ duplicatedState2);

            offset += 64;
            len -= 64;
        } while (len > 64);

        currentState = currentState ^ duplicatedState0 ^ (duplicatedState1 + duplicatedState2);

        if (len > 32)
            currentState = Mix32Bytes(data, offset, currentState);

        return Mix32Bytes(data, last32, currentState);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Mix(ulong lhs, ulong rhs) => Fold128To64(lhs, rhs);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong CombineRaw(ulong state, ulong value) => Mix(state ^ value, Mul);

#if NET8_0_OR_GREATER
    internal static bool IsAesSupported => Aes.IsSupported && Sse2.IsSupported && Sse42.IsSupported;

    internal static ulong CombineLargeContiguousSimd(ulong state, ReadOnlySpan<byte> data)
    {
        int len = data.Length;

        if (len <= PiecewiseChunkSize)
            return LowLevelHashLenGt32Simd(state, data, len);

        while (len >= PiecewiseChunkSize)
        {
            state = LowLevelHashLenGt32Simd(state, data.Slice(0, PiecewiseChunkSize), PiecewiseChunkSize);
            data = data.Slice(PiecewiseChunkSize);
            len -= PiecewiseChunkSize;
        }

        return len == 0 ? state : AbslHash64.CombineContiguous(state, data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong LowLevelHashLenGt32Simd(ulong seed, ReadOnlySpan<byte> data, int len) => len > 64 ? LowLevelHashLenGt64Simd(seed, data, len) : LowLevelHash33To64Simd(seed, data, len);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong LowLevelHash33To64Simd(ulong seed, ReadOnlySpan<byte> data, int len)
    {
        Vector128<byte> state = Set128(seed, (ulong)len);
        Vector128<byte> a = Load128(data, 0);
        Vector128<byte> b = Load128(data, 16);
        int last32 = len - 32;
        Vector128<byte> c = Load128(data, last32);
        Vector128<byte> d = Load128(data, last32 + 16);

        Vector128<byte> na = MixA(a, state);
        Vector128<byte> nb = MixB(b, state);
        Vector128<byte> nc = MixC(c, state);
        Vector128<byte> nd = MixD(d, state);

        return Mix4x16Vectors(na, nb, nc, nd);
    }

    private static ulong LowLevelHashLenGt64Simd(ulong seed, ReadOnlySpan<byte> data, int len)
    {
        int last32 = len - 32;
        int offset = 0;

        Vector128<byte> state0 = Set128(seed, (ulong)len);
        Vector128<byte> state1 = state0;
        Vector128<byte> state2 = state1;
        Vector128<byte> state3 = state2;

        do
        {
            MixAb(data, offset, ref state0, ref state1);
            MixCd(data, offset + 32, ref state2, ref state3);

            offset += 64;
            len -= 64;
        } while (len > 64);

        if (len > 32)
            MixAb(data, offset, ref state0, ref state1);

        MixCd(data, last32, ref state2, ref state3);

        return Mix4x16Vectors(state0, state1, state2, state3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MixAb(ReadOnlySpan<byte> data, int offset, ref Vector128<byte> state0, ref Vector128<byte> state1)
    {
        Vector128<byte> a = Load128(data, offset);
        Vector128<byte> b = Load128(data, offset + 16);
        state0 = MixA(a, state0);
        state1 = MixB(b, state1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MixCd(ReadOnlySpan<byte> data, int offset, ref Vector128<byte> state2, ref Vector128<byte> state3)
    {
        Vector128<byte> c = Load128(data, offset);
        Vector128<byte> d = Load128(data, offset + 16);
        state2 = MixC(c, state2);
        state3 = MixD(d, state3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Load128(ReadOnlySpan<byte> data, int offset)
    {
        ref byte ptr = ref MemoryMarshal.GetReference(data);
        return Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Set128(ulong a, ulong b) => Vector128.Create(b, a).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Add128(Vector128<byte> a, Vector128<byte> b) => Sse2.Add(a.AsUInt64(), b.AsUInt64()).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Sub128(Vector128<byte> a, Vector128<byte> b) => Sse2.Subtract(a.AsUInt64(), b.AsUInt64()).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> MixA(Vector128<byte> a, Vector128<byte> state) => Aes.Decrypt(Add128(state, a), state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> MixB(Vector128<byte> b, Vector128<byte> state) => Aes.Decrypt(Sub128(state, b), state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> MixC(Vector128<byte> c, Vector128<byte> state) => Aes.Encrypt(Add128(state, c), state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> MixD(Vector128<byte> d, Vector128<byte> state) => Aes.Encrypt(Sub128(state, d), state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Mix4x16Vectors(Vector128<byte> a, Vector128<byte> b, Vector128<byte> c, Vector128<byte> d)
    {
        Vector128<byte> res = Add128(Aes.Encrypt(Add128(a, c), d), Aes.Decrypt(Sub128(b, d), a));
        Vector128<ulong> res64 = res.AsUInt64();
        return res64.GetElement(0) ^ res64.GetElement(1);
    }

#endif
}