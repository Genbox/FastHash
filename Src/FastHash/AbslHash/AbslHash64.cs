// C# port of the Abseil low-level hash from abseil/abseil-cpp (ddbc605). Copyright 2018 The Abseil Authors.
// Distributed under the Apache License 2.0; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Genbox.FastHash.AbslHash.AbslHashConstants;

#if NET8_0_OR_GREATER
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
    private static ulong PrecombineLengthMix(ulong state, int len) => state ^ Read64(ref MemoryMarshal.GetReference(StaticRandomData), len);

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
    private static ulong CombineContiguous9To16(ulong state, ReadOnlySpan<byte> data, int len) => Mix(state ^ Read64(ref MemoryMarshal.GetReference(data)), Mul ^ Read64(ref MemoryMarshal.GetReference(data), len - 8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong CombineContiguous17To32(ulong state, ReadOnlySpan<byte> data, int len)
    {
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        ulong m0 = Mix(Read64(ref dataReference) ^ 0x13198a2e03707344UL, Read64(ref dataReference, 8) ^ state);
        int tail16 = len - 16;
        ulong m1 = Mix(Read64(ref dataReference, tail16) ^ 0x082efa98ec4e6c89UL, Read64(ref dataReference, tail16 + 8) ^ state);
        return m0 ^ m1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Read4To8(ReadOnlySpan<byte> data, int len)
    {
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        ulong mostSignificant = (ulong)Read32(ref dataReference) << 32;
        ulong leastSignificant = Read32(ref dataReference, len - 4);
        return mostSignificant | leastSignificant;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Read1To3(ReadOnlySpan<byte> data, int len)
    {
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        uint mem0 = ((uint)Unsafe.Add(ref dataReference, 0) << 16) | Unsafe.Add(ref dataReference, len - 1);
        uint mem1 = (uint)Unsafe.Add(ref dataReference, len / 2) << 8;
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
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        ulong a = Read64(ref dataReference, offset);
        ulong b = Read64(ref dataReference, offset + 8);
        ulong c = Read64(ref dataReference, offset + 16);
        ulong d = Read64(ref dataReference, offset + 24);

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
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        ulong currentState = seed ^ 0x243f6a8885a308d3UL ^ (uint)len;
        int last32 = len - 32;
        ulong duplicatedState0 = currentState;
        ulong duplicatedState1 = currentState;
        ulong duplicatedState2 = currentState;
        int offset = 0;

        do
        {
            ulong a = Read64(ref dataReference, offset);
            ulong b = Read64(ref dataReference, offset + 8);
            ulong c = Read64(ref dataReference, offset + 16);
            ulong d = Read64(ref dataReference, offset + 24);
            ulong e = Read64(ref dataReference, offset + 32);
            ulong f = Read64(ref dataReference, offset + 40);
            ulong g = Read64(ref dataReference, offset + 48);
            ulong h = Read64(ref dataReference, offset + 56);

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

    // Keep rounds local: deep helper chains can leave vector-argument calls in callers.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong LowLevelHashLenGt64Simd(ulong seed, ReadOnlySpan<byte> data, int len)
    {
        int last32 = len - 32;
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        ref byte current = ref dataReference;
        Vector128<byte> state0 = Set128(seed, (ulong)len);
        Vector128<byte> state1 = state0;
        Vector128<byte> state2 = state0;
        Vector128<byte> state3 = state0;

        do
        {
            Vector128<byte> a = Unsafe.ReadUnaligned<Vector128<byte>>(ref current);
            Vector128<byte> b = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref current, 16));
            state0 = Aes.Decrypt(Sse2.Add(state0.AsUInt64(), a.AsUInt64()).AsByte(), state0);
            state1 = Aes.Decrypt(Sse2.Subtract(state1.AsUInt64(), b.AsUInt64()).AsByte(), state1);
            Vector128<byte> c = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref current, 32));
            Vector128<byte> d = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref current, 48));
            state2 = Aes.Encrypt(Sse2.Add(state2.AsUInt64(), c.AsUInt64()).AsByte(), state2);
            state3 = Aes.Encrypt(Sse2.Subtract(state3.AsUInt64(), d.AsUInt64()).AsByte(), state3);
            current = ref Unsafe.Add(ref current, 64);
            len -= 64;
        } while (len > 64);

        if (len > 32)
        {
            Vector128<byte> extraA = Unsafe.ReadUnaligned<Vector128<byte>>(ref current);
            Vector128<byte> extraB = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref current, 16));
            state0 = Aes.Decrypt(Sse2.Add(state0.AsUInt64(), extraA.AsUInt64()).AsByte(), state0);
            state1 = Aes.Decrypt(Sse2.Subtract(state1.AsUInt64(), extraB.AsUInt64()).AsByte(), state1);
        }

        ref byte tail = ref Unsafe.Add(ref dataReference, last32);
        Vector128<byte> tailC = Unsafe.ReadUnaligned<Vector128<byte>>(ref tail);
        Vector128<byte> tailD = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref tail, 16));
        state2 = Aes.Encrypt(Sse2.Add(state2.AsUInt64(), tailC.AsUInt64()).AsByte(), state2);
        state3 = Aes.Encrypt(Sse2.Subtract(state3.AsUInt64(), tailD.AsUInt64()).AsByte(), state3);
        Vector128<byte> left = Aes.Encrypt(Sse2.Add(state0.AsUInt64(), state2.AsUInt64()).AsByte(), state3);
        Vector128<byte> right = Aes.Decrypt(Sse2.Subtract(state1.AsUInt64(), state3.AsUInt64()).AsByte(), state0);
        Vector128<ulong> result = Sse2.Add(left.AsUInt64(), right.AsUInt64());
        return result.GetElement(0) ^ result.GetElement(1);
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
    private static Vector128<byte> MixA(Vector128<byte> a, Vector128<byte> state) => Aes.Decrypt(Sse2.Add(state.AsUInt64(), a.AsUInt64()).AsByte(), state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> MixB(Vector128<byte> b, Vector128<byte> state) => Aes.Decrypt(Sse2.Subtract(state.AsUInt64(), b.AsUInt64()).AsByte(), state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> MixC(Vector128<byte> c, Vector128<byte> state) => Aes.Encrypt(Sse2.Add(state.AsUInt64(), c.AsUInt64()).AsByte(), state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> MixD(Vector128<byte> d, Vector128<byte> state) => Aes.Encrypt(Sse2.Subtract(state.AsUInt64(), d.AsUInt64()).AsByte(), state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Mix4x16Vectors(Vector128<byte> a, Vector128<byte> b, Vector128<byte> c, Vector128<byte> d)
    {
        Vector128<byte> left = Aes.Encrypt(Sse2.Add(a.AsUInt64(), c.AsUInt64()).AsByte(), d);
        Vector128<byte> right = Aes.Decrypt(Sse2.Subtract(b.AsUInt64(), d.AsUInt64()).AsByte(), a);
        Vector128<byte> res = Sse2.Add(left.AsUInt64(), right.AsUInt64()).AsByte();
        Vector128<ulong> res64 = res.AsUInt64();
        return res64.GetElement(0) ^ res64.GetElement(1);
    }

#endif
}