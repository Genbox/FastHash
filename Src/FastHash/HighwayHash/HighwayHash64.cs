// C# port of google/highwayhash (f8381f3). Copyright 2017 Google Inc. All Rights Reserved.
// Distributed under the Apache License 2.0; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET8_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace Genbox.FastHash.HighwayHash;

/// <summary>Provides the 64-bit HighwayHash algorithm.</summary>
public static class HighwayHash64
{
    /// <summary>Computes the hash of a 64-bit value using the default key.</summary>
    /// <param name="input">The value to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input) => ComputeIndex(input, HighwayHashConstants.DefaultKey0, HighwayHashConstants.DefaultKey1, HighwayHashConstants.DefaultKey2, HighwayHashConstants.DefaultKey3);

    /// <summary>Computes the hash of a 64-bit value.</summary>
    /// <param name="input">The value to hash.</param>
    /// <param name="seed1">The first key word.</param>
    /// <param name="seed2">The second key word.</param>
    /// <param name="seed3">The third key word.</param>
    /// <param name="seed4">The fourth key word.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed1, ulong seed2, ulong seed3, ulong seed4)
    {
#if NET8_0_OR_GREATER
        if (IsAvx2Supported)
            return HashIndexAvx2(input, seed1, seed2, seed3, seed4);
#endif

        HighwayHashState state = new HighwayHashState();
        Reset(seed1, seed2, seed3, seed4, ref state);
        UpdateIndex(input, ref state);
        return Finalize64(state);
    }

    /// <summary>Computes the hash of a 64-bit value.</summary>
    /// <param name="input">The value to hash.</param>
    /// <param name="keys">The key words; the first four are used.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="keys" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="keys" /> has fewer than four elements.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong[] keys)
    {
        ValidateKeys(keys);
        return ComputeIndex(input, keys[0], keys[1], keys[2], keys[3]);
    }

    /// <summary>Computes the hash of a byte sequence using the default key.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, HighwayHashConstants.DefaultKey0, HighwayHashConstants.DefaultKey1, HighwayHashConstants.DefaultKey2, HighwayHashConstants.DefaultKey3);

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="seed1">The first key word.</param>
    /// <param name="seed2">The second key word.</param>
    /// <param name="seed3">The third key word.</param>
    /// <param name="seed4">The fourth key word.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed1, ulong seed2, ulong seed3, ulong seed4)
    {
#if NET8_0_OR_GREATER
        if (IsAvx2Supported)
            return HashAvx2(data, seed1, seed2, seed3, seed4);
#endif

        HighwayHashState state = new HighwayHashState();
        ProcessAll(data, seed1, seed2, seed3, seed4, ref state);
        return Finalize64(state);
    }

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="keys">The key words; the first four are used.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="keys" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="keys" /> has fewer than four elements.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong[] keys)
    {
        ValidateKeys(keys);
        return ComputeHash(data, keys[0], keys[1], keys[2], keys[3]);
    }

    internal static void ValidateKeys(ulong[]? keys)
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        if (keys.Length < 4)
            throw new ArgumentException("HighwayHash keys must contain at least four 64-bit words.", nameof(keys));
    }

    private static void ProcessAll(ReadOnlySpan<byte> data, ulong key0, ulong key1, ulong key2, ulong key3, ref HighwayHashState state)
    {
        int size = data.Length;
        Reset(key0, key1, key2, key3, ref state);

        int i;
        for (i = 0; i + 32 <= size; i += 32)
            UpdatePacket(data, i, ref state);

        int remainder = size & 31;
        if (remainder != 0)
            UpdateRemainder(data.Slice(i, remainder), remainder, ref state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdatePacket(ReadOnlySpan<byte> packet, int offset, ref HighwayHashState state) => Update(Read64(ref MemoryMarshal.GetReference(packet), offset), Read64(ref MemoryMarshal.GetReference(packet), offset + 8), Read64(ref MemoryMarshal.GetReference(packet), offset + 16), Read64(ref MemoryMarshal.GetReference(packet), offset + 24), ref state);

    private static void UpdateRemainder(ReadOnlySpan<byte> bytes, int sizeMod32, ref HighwayHashState state)
    {
        ref byte bytesReference = ref MemoryMarshal.GetReference(bytes);
        int sizeMod4 = sizeMod32 & 3;
        int remainderOffset = sizeMod32 & ~3;
        Span<byte> packet = stackalloc byte[32];
        packet.Clear();

        state.v0_0 += ((ulong)sizeMod32 << 32) + (uint)sizeMod32;
        state.v0_1 += ((ulong)sizeMod32 << 32) + (uint)sizeMod32;
        state.v0_2 += ((ulong)sizeMod32 << 32) + (uint)sizeMod32;
        state.v0_3 += ((ulong)sizeMod32 << 32) + (uint)sizeMod32;

        uint half0 = (uint)(state.v1_0 & 0xffffffff);
        uint half1 = (uint)(state.v1_0 >> 32);
        state.v1_0 = (half0 << sizeMod32) | (half0 >> (32 - sizeMod32));
        state.v1_0 |= (ulong)((half1 << sizeMod32) | (half1 >> (32 - sizeMod32))) << 32;

        half0 = (uint)(state.v1_1 & 0xffffffff);
        half1 = (uint)(state.v1_1 >> 32);
        state.v1_1 = (half0 << sizeMod32) | (half0 >> (32 - sizeMod32));
        state.v1_1 |= (ulong)((half1 << sizeMod32) | (half1 >> (32 - sizeMod32))) << 32;

        half0 = (uint)(state.v1_2 & 0xffffffff);
        half1 = (uint)(state.v1_2 >> 32);
        state.v1_2 = (half0 << sizeMod32) | (half0 >> (32 - sizeMod32));
        state.v1_2 |= (ulong)((half1 << sizeMod32) | (half1 >> (32 - sizeMod32))) << 32;

        half0 = (uint)(state.v1_3 & 0xffffffff);
        half1 = (uint)(state.v1_3 >> 32);
        state.v1_3 = (half0 << sizeMod32) | (half0 >> (32 - sizeMod32));
        state.v1_3 |= (ulong)((half1 << sizeMod32) | (half1 >> (32 - sizeMod32))) << 32;

        bytes.Slice(0, remainderOffset).CopyTo(packet);

        if ((sizeMod32 & 16) != 0)
        {
            for (int i = 0; i < 4; i++)
                Unsafe.Add(ref MemoryMarshal.GetReference(packet), 28 + i) = Unsafe.Add(ref bytesReference, (remainderOffset + i + sizeMod4) - 4);
        }
        else if (sizeMod4 != 0)
        {
            Unsafe.Add(ref MemoryMarshal.GetReference(packet), 16) = Unsafe.Add(ref bytesReference, remainderOffset);
            Unsafe.Add(ref MemoryMarshal.GetReference(packet), 17) = Unsafe.Add(ref bytesReference, remainderOffset + (sizeMod4 >> 1));
            Unsafe.Add(ref MemoryMarshal.GetReference(packet), 18) = Unsafe.Add(ref bytesReference, (remainderOffset + sizeMod4) - 1);
        }

        UpdatePacket(packet, 0, ref state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Reset(ulong key0, ulong key1, ulong key2, ulong key3, ref HighwayHashState state)
    {
        state.mul0_0 = 0xdbe6d5d5fe4cce2ful;
        state.mul0_1 = 0xa4093822299f31d0ul;
        state.mul0_2 = 0x13198a2e03707344ul;
        state.mul0_3 = 0x243f6a8885a308d3ul;
        state.mul1_0 = 0x3bd39e10cb0ef593ul;
        state.mul1_1 = 0xc0acf169b5f18a8cul;
        state.mul1_2 = 0xbe5466cf34e90c6cul;
        state.mul1_3 = 0x452821e638d01377ul;
        state.v0_0 = state.mul0_0 ^ key0;
        state.v0_1 = state.mul0_1 ^ key1;
        state.v0_2 = state.mul0_2 ^ key2;
        state.v0_3 = state.mul0_3 ^ key3;
        state.v1_0 = state.mul1_0 ^ ((key0 >> 32) | (key0 << 32));
        state.v1_1 = state.mul1_1 ^ ((key1 >> 32) | (key1 << 32));
        state.v1_2 = state.mul1_2 ^ ((key2 >> 32) | (key2 << 32));
        state.v1_3 = state.mul1_3 ^ ((key3 >> 32) | (key3 << 32));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Update(ulong lane0, ulong lane1, ulong lane2, ulong lane3, ref HighwayHashState state)
    {
        state.v1_0 += state.mul0_0 + lane0;
        state.mul0_0 ^= (state.v1_0 & 0xffffffff) * (state.v0_0 >> 32);
        state.v0_0 += state.mul1_0;
        state.mul1_0 ^= (state.v0_0 & 0xffffffff) * (state.v1_0 >> 32);

        state.v1_1 += state.mul0_1 + lane1;
        state.mul0_1 ^= (state.v1_1 & 0xffffffff) * (state.v0_1 >> 32);
        state.v0_1 += state.mul1_1;
        state.mul1_1 ^= (state.v0_1 & 0xffffffff) * (state.v1_1 >> 32);

        state.v1_2 += state.mul0_2 + lane2;
        state.mul0_2 ^= (state.v1_2 & 0xffffffff) * (state.v0_2 >> 32);
        state.v0_2 += state.mul1_2;
        state.mul1_2 ^= (state.v0_2 & 0xffffffff) * (state.v1_2 >> 32);

        state.v1_3 += state.mul0_3 + lane3;
        state.mul0_3 ^= (state.v1_3 & 0xffffffff) * (state.v0_3 >> 32);
        state.v0_3 += state.mul1_3;
        state.mul1_3 ^= (state.v0_3 & 0xffffffff) * (state.v1_3 >> 32);

        ZipperMergeAndAdd(state.v1_1, state.v1_0, ref state.v0_1, ref state.v0_0);
        ZipperMergeAndAdd(state.v1_3, state.v1_2, ref state.v0_3, ref state.v0_2);
        ZipperMergeAndAdd(state.v0_1, state.v0_0, ref state.v1_1, ref state.v1_0);
        ZipperMergeAndAdd(state.v0_3, state.v0_2, ref state.v1_3, ref state.v1_2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateIndex(ulong input, ref HighwayHashState state)
    {
        const ulong length = 0x0000000800000008UL;
        state.v0_0 += length;
        state.v0_1 += length;
        state.v0_2 += length;
        state.v0_3 += length;

        state.v1_0 = RotateHalvesLeft8(state.v1_0);
        state.v1_1 = RotateHalvesLeft8(state.v1_1);
        state.v1_2 = RotateHalvesLeft8(state.v1_2);
        state.v1_3 = RotateHalvesLeft8(state.v1_3);

        Update(input, 0, 0, 0, ref state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotateHalvesLeft8(ulong value)
    {
        uint low = RotateLeft((uint)value, 8);
        uint high = RotateLeft((uint)(value >> 32), 8);
        return low | ((ulong)high << 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ZipperMergeAndAdd(ulong v1, ulong v0, ref ulong add1, ref ulong add0)
    {
        add0 += (((v0 & 0xff000000ul) | (v1 & 0xff00000000ul)) >> 24) |
                (((v0 & 0xff0000000000ul) | (v1 & 0xff000000000000ul)) >> 16) |
                (v0 & 0xff0000ul) | ((v0 & 0xff00ul) << 32) |
                ((v1 & 0xff00000000000000ul) >> 8) | (v0 << 56);

        add1 += (((v1 & 0xff000000ul) | (v0 & 0xff00000000ul)) >> 24) |
                (v1 & 0xff0000ul) | ((v1 & 0xff0000000000ul) >> 16) |
                ((v1 & 0xff00ul) << 24) | ((v0 & 0xff000000000000ul) >> 8) |
                ((v1 & 0xfful) << 48) | (v0 & 0xff00000000000000ul);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Finalize64(HighwayHashState state)
    {
        for (int i = 0; i < 4; i++)
            PermuteAndUpdate(ref state);

        return state.v0_0 + state.v1_0 + state.mul0_0 + state.mul1_0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PermuteAndUpdate(ref HighwayHashState state) => Update((state.v0_2 >> 32) | (state.v0_2 << 32),
        (state.v0_3 >> 32) | (state.v0_3 << 32),
        (state.v0_0 >> 32) | (state.v0_0 << 32),
        (state.v0_1 >> 32) | (state.v0_1 << 32),
        ref state);

#if NET8_0_OR_GREATER
    internal static bool IsAvx2Supported => Avx2.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong HashAvx2(ReadOnlySpan<byte> data, ulong key0, ulong key1, ulong key2, ulong key3) =>
        HashAvx2(ref MemoryMarshal.GetReference(data), data.Length, key0, key1, key2, key3);

    internal static ulong HashAvx2(ref byte data, int size, ulong key0, ulong key1, ulong key2, ulong key3)
    {
        ResetAvx2(key0, key1, key2, key3, out Vector256<ulong> v0, out Vector256<ulong> v1, out Vector256<ulong> mul0, out Vector256<ulong> mul1);

        int i = 0;
        for (; i + 32 <= size; i += 32)
            UpdateAvx2(Vector256.LoadUnsafe(ref data, (nuint)i).AsUInt64(), ref v0, ref v1, ref mul0, ref mul1);

        int remainder = size & 31;
        if (remainder != 0)
            UpdateRemainderAvx2(ref Unsafe.Add(ref data, i), remainder, ref v0, ref v1, ref mul0, ref mul1);

        return Finalize64Avx2(v0, v1, mul0, mul1);
    }

    internal static ulong HashIndexAvx2(ulong input, ulong key0, ulong key1, ulong key2, ulong key3)
    {
        ResetAvx2(key0, key1, key2, key3, out Vector256<ulong> v0, out Vector256<ulong> v1, out Vector256<ulong> mul0, out Vector256<ulong> mul1);

        // Same as UpdateRemainderAvx2 for an 8-byte input: the packet is the input followed by zeros.
        v0 += Vector256.Create(0x0000000800000008UL);
        v1 = Rotate32By(v1, 8);
        UpdateAvx2(Vector256.Create(input, 0UL, 0UL, 0UL), ref v0, ref v1, ref mul0, ref mul1);
        return Finalize64Avx2(v0, v1, mul0, mul1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ResetAvx2(ulong key0, ulong key1, ulong key2, ulong key3, out Vector256<ulong> v0, out Vector256<ulong> v1, out Vector256<ulong> mul0, out Vector256<ulong> mul1)
    {
        mul0 = Vector256.Create(0xdbe6d5d5fe4cce2fUL, 0xa4093822299f31d0UL, 0x13198a2e03707344UL, 0x243f6a8885a308d3UL);
        mul1 = Vector256.Create(0x3bd39e10cb0ef593UL, 0xc0acf169b5f18a8cUL, 0xbe5466cf34e90c6cUL, 0x452821e638d01377UL);
        Vector256<ulong> key = Vector256.Create(key0, key1, key2, key3);
        v0 = key ^ mul0;
        v1 = Rotate64By32(key) ^ mul1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateAvx2(Vector256<ulong> packet, ref Vector256<ulong> v0, ref Vector256<ulong> v1, ref Vector256<ulong> mul0, ref Vector256<ulong> mul1)
    {
        v1 += packet;
        v1 += mul0;
        mul0 ^= Avx2.Multiply(v1.AsUInt32(), Avx2.ShiftRightLogical(v0, 32).AsUInt32());
        v0 += mul1;
        mul1 ^= Avx2.Multiply(v0.AsUInt32(), Avx2.ShiftRightLogical(v1, 32).AsUInt32());
        v0 += ZipperMerge(v1);
        v1 += ZipperMerge(v0);
    }

    private static void UpdateRemainderAvx2(ref byte bytes, int sizeMod32, ref Vector256<ulong> v0, ref Vector256<ulong> v1, ref Vector256<ulong> mul0, ref Vector256<ulong> mul1)
    {
        v0 += Vector256.Create(((ulong)sizeMod32 << 32) + (uint)sizeMod32);
        v1 = Rotate32By(v1, sizeMod32);

        // Same packet layout as the portable implementation.
        int sizeMod4 = sizeMod32 & 3;
        int remainderOffset = sizeMod32 & ~3;
        Span<byte> packet = stackalloc byte[32];
        packet.Clear();
        Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(packet), ref bytes, (uint)remainderOffset);

        if ((sizeMod32 & 16) != 0)
        {
            for (int i = 0; i < 4; i++)
                Unsafe.Add(ref MemoryMarshal.GetReference(packet), 28 + i) = Unsafe.Add(ref bytes, (remainderOffset + i + sizeMod4) - 4);
        }
        else if (sizeMod4 != 0)
        {
            Unsafe.Add(ref MemoryMarshal.GetReference(packet), 16) = Unsafe.Add(ref bytes, remainderOffset);
            Unsafe.Add(ref MemoryMarshal.GetReference(packet), 17) = Unsafe.Add(ref bytes, remainderOffset + (sizeMod4 >> 1));
            Unsafe.Add(ref MemoryMarshal.GetReference(packet), 18) = Unsafe.Add(ref bytes, (remainderOffset + sizeMod4) - 1);
        }

        UpdateAvx2(Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(packet)).AsUInt64(), ref v0, ref v1, ref mul0, ref mul1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Finalize64Avx2(Vector256<ulong> v0, Vector256<ulong> v1, Vector256<ulong> mul0, Vector256<ulong> mul1)
    {
        for (int i = 0; i < 4; i++)
            UpdateAvx2(Permute(v0), ref v0, ref v1, ref mul0, ref mul1);

        return (v0 + mul0 + v1 + mul1).GetElement(0);
    }

    // Swaps the 32-bit halves of each 64-bit lane.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ulong> Rotate64By32(Vector256<ulong> v) => Avx2.Shuffle(v.AsUInt32(), 0b10_11_00_01).AsUInt64();

    // Rotates each 32-bit half left by count (1..31).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ulong> Rotate32By(Vector256<ulong> v, int count)
    {
        Vector256<uint> u = v.AsUInt32();
        return (Vector256.ShiftLeft(u, count) | Vector256.ShiftRightLogical(u, 32 - count)).AsUInt64();
    }

    // Lanes become (v2 halves swapped, v3 swapped, v0 swapped, v1 swapped), as in the portable PermuteAndUpdate.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ulong> Permute(Vector256<ulong> v) => Avx2.PermuteVar8x32(v.AsUInt32(), Vector256.Create(5u, 4u, 7u, 6u, 1u, 0u, 3u, 2u)).AsUInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ulong> ZipperMerge(Vector256<ulong> v)
    {
        Vector256<byte> mask = Vector256.Create((byte)0x03, 0x0C, 0x02, 0x05, 0x0E, 0x01, 0x0F, 0x00, 0x0B, 0x04, 0x0A, 0x0D, 0x09, 0x06, 0x08, 0x07,
            0x03, 0x0C, 0x02, 0x05, 0x0E, 0x01, 0x0F, 0x00, 0x0B, 0x04, 0x0A, 0x0D, 0x09, 0x06, 0x08, 0x07);
        return Avx2.Shuffle(v.AsByte(), mask).AsUInt64();
    }

#endif

    [StructLayout(LayoutKind.Auto)]
    private struct HighwayHashState
    {
        internal ulong mul0_0;
        internal ulong mul0_1;
        internal ulong mul0_2;
        internal ulong mul0_3;
        internal ulong mul1_0;
        internal ulong mul1_1;
        internal ulong mul1_2;
        internal ulong mul1_3;
        internal ulong v0_0;
        internal ulong v0_1;
        internal ulong v0_2;
        internal ulong v0_3;
        internal ulong v1_0;
        internal ulong v1_1;
        internal ulong v1_2;
        internal ulong v1_3;
    }
}