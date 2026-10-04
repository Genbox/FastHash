#if NET8_0_OR_GREATER

// C# port of google/highwayhash (f8381f3). Copyright 2017 Google Inc. All Rights Reserved.
// Distributed under the Apache License 2.0; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Genbox.FastHash.HighwayHash;

// Port of upstream's hh_avx2.h. Each Vector256 holds the four 64-bit lanes that the portable
// implementation keeps in separate fields (lane 0 = element 0), so the output is identical.
internal static class HighwayHashAvx2
{
    internal static bool IsSupported => Avx2.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Hash(ReadOnlySpan<byte> data, ulong key0, ulong key1, ulong key2, ulong key3) =>
        Hash(ref MemoryMarshal.GetReference(data), data.Length, key0, key1, key2, key3);

    internal static ulong Hash(ref byte data, int size, ulong key0, ulong key1, ulong key2, ulong key3)
    {
        Reset(key0, key1, key2, key3, out Vector256<ulong> v0, out Vector256<ulong> v1, out Vector256<ulong> mul0, out Vector256<ulong> mul1);

        int i = 0;
        for (; i + 32 <= size; i += 32)
            Update(Vector256.LoadUnsafe(ref data, (nuint)i).AsUInt64(), ref v0, ref v1, ref mul0, ref mul1);

        int remainder = size & 31;
        if (remainder != 0)
            UpdateRemainder(ref Unsafe.Add(ref data, i), remainder, ref v0, ref v1, ref mul0, ref mul1);

        return Finalize64(v0, v1, mul0, mul1);
    }

    internal static ulong HashIndex(ulong input, ulong key0, ulong key1, ulong key2, ulong key3)
    {
        Reset(key0, key1, key2, key3, out Vector256<ulong> v0, out Vector256<ulong> v1, out Vector256<ulong> mul0, out Vector256<ulong> mul1);

        // Same as UpdateRemainder for an 8-byte input: the packet is the input followed by zeros.
        v0 += Vector256.Create(0x0000000800000008UL);
        v1 = Rotate32By(v1, 8);
        Update(Vector256.Create(input, 0UL, 0UL, 0UL), ref v0, ref v1, ref mul0, ref mul1);
        return Finalize64(v0, v1, mul0, mul1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Reset(ulong key0, ulong key1, ulong key2, ulong key3, out Vector256<ulong> v0, out Vector256<ulong> v1, out Vector256<ulong> mul0, out Vector256<ulong> mul1)
    {
        mul0 = Vector256.Create(0xdbe6d5d5fe4cce2fUL, 0xa4093822299f31d0UL, 0x13198a2e03707344UL, 0x243f6a8885a308d3UL);
        mul1 = Vector256.Create(0x3bd39e10cb0ef593UL, 0xc0acf169b5f18a8cUL, 0xbe5466cf34e90c6cUL, 0x452821e638d01377UL);
        Vector256<ulong> key = Vector256.Create(key0, key1, key2, key3);
        v0 = key ^ mul0;
        v1 = Rotate64By32(key) ^ mul1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Update(Vector256<ulong> packet, ref Vector256<ulong> v0, ref Vector256<ulong> v1, ref Vector256<ulong> mul0, ref Vector256<ulong> mul1)
    {
        v1 += packet;
        v1 += mul0;
        mul0 ^= Avx2.Multiply(v1.AsUInt32(), Avx2.ShiftRightLogical(v0, 32).AsUInt32());
        v0 += mul1;
        mul1 ^= Avx2.Multiply(v0.AsUInt32(), Avx2.ShiftRightLogical(v1, 32).AsUInt32());
        v0 += ZipperMerge(v1);
        v1 += ZipperMerge(v0);
    }

    private static void UpdateRemainder(ref byte bytes, int sizeMod32, ref Vector256<ulong> v0, ref Vector256<ulong> v1, ref Vector256<ulong> mul0, ref Vector256<ulong> mul1)
    {
        v0 += Vector256.Create(((ulong)sizeMod32 << 32) + (uint)sizeMod32);
        v1 = Rotate32By(v1, sizeMod32);

        // Same packet layout as the portable implementation.
        int sizeMod4 = sizeMod32 & 3;
        int remainderOffset = sizeMod32 & ~3;
        Span<byte> packet = stackalloc byte[32];
        packet.Clear();
        MemoryMarshal.CreateReadOnlySpan(ref bytes, remainderOffset).CopyTo(packet);

        if ((sizeMod32 & 16) != 0)
        {
            for (int i = 0; i < 4; i++)
                packet[28 + i] = Unsafe.Add(ref bytes, (remainderOffset + i + sizeMod4) - 4);
        }
        else if (sizeMod4 != 0)
        {
            packet[16] = Unsafe.Add(ref bytes, remainderOffset);
            packet[17] = Unsafe.Add(ref bytes, remainderOffset + (sizeMod4 >> 1));
            packet[18] = Unsafe.Add(ref bytes, (remainderOffset + sizeMod4) - 1);
        }

        Update(Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(packet)).AsUInt64(), ref v0, ref v1, ref mul0, ref mul1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Finalize64(Vector256<ulong> v0, Vector256<ulong> v1, Vector256<ulong> mul0, Vector256<ulong> mul1)
    {
        for (int i = 0; i < 4; i++)
            Update(Permute(v0), ref v0, ref v1, ref mul0, ref mul1);

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
}
#endif