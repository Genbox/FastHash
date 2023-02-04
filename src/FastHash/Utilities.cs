using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Genbox.FastHash;

internal static class Utilities
{
    #region Unsafe read/write

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe uint Read8(byte* ptr) => *ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe ushort Read16(byte* ptr) => *(ushort*)ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe uint Read32(byte* ptr) => *(uint*)ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe ulong Read64(byte* ptr) => *(ulong*)ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe void Write64(byte* ptr, ulong value) => *(ulong*)ptr = value;

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort Read16(ReadOnlySpan<byte> data, int offset)
    {
        ref byte ptr = ref MemoryMarshal.GetReference(data);
        return Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Read32(ReadOnlySpan<byte> data)
    {
        ref byte ptr = ref MemoryMarshal.GetReference(data);
        return Unsafe.ReadUnaligned<uint>(ref ptr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Read32(ReadOnlySpan<byte> data, uint offset)
    {
        ref byte ptr = ref MemoryMarshal.GetReference(data);
        return Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref ptr, (IntPtr)offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Read32(ReadOnlySpan<byte> data, int offset)
    {
        ref byte ptr = ref MemoryMarshal.GetReference(data);
        return Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Read64(ReadOnlySpan<byte> data)
    {
        ref byte ptr = ref MemoryMarshal.GetReference(data);
        return Unsafe.ReadUnaligned<ulong>(ref ptr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Read64(ReadOnlySpan<byte> data, uint offset)
    {
        ref byte ptr = ref MemoryMarshal.GetReference(data);
        return Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref ptr, (IntPtr)offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Read64(ReadOnlySpan<byte> data, int offset)
    {
        ref byte ptr = ref MemoryMarshal.GetReference(data);
        return Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Write64(Span<byte> data, int offset, ulong value) => Unsafe.WriteUnaligned(ref data[offset], value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint ByteSwap(uint input) => BinaryPrimitives.ReverseEndianness(input);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong ByteSwap(ulong input) => BinaryPrimitives.ReverseEndianness(input);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint RotateRight(uint x, byte r) => (x >> r) | (x << (32 - r));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong RotateRight(ulong x, byte r) => (x >> r) | (x << (64 - r));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint RotateLeft(uint x, byte r) => (x << r) | (x >> (32 - r));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong RotateLeft(ulong x, byte r) => (x << r) | (x >> (64 - r));

    public static ulong BigMul(ulong a, ulong b, out ulong low)
    {
        // Adaptation of algorithm for multiplication of 32-bit unsigned integers described
        // in Hacker's Delight by Henry S. Warren, Jr. (ISBN 0-201-91465-4), Chapter 8
        // Basically, it's an optimized version of FOIL method applied to low and high dwords of each operand

        // Use 32-bit uints to optimize the fallback for 32-bit platforms.
        uint al = (uint)a;
        uint ah = (uint)(a >> 32);
        uint bl = (uint)b;
        uint bh = (uint)(b >> 32);

        ulong mull = (ulong)al * bl;
        ulong t = (ulong)ah * bl + (mull >> 32);
        ulong tl = (ulong)al * bh + (uint)t;

        low = tl << 32 | (uint)mull;

        return (ulong)ah * bh + (t >> 32) + (tl >> 32);
    }
}