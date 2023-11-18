using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Genbox.FastHash.Misc;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong BigMul(ulong a, ulong b, out ulong low)
    {
#if NET5_0_OR_GREATER
        return Math.BigMul(a, b, out low);
#else
        unchecked
        {
            low = a * b;

            ulong x0 = (uint)a;
            ulong x1 = a >> 32;

            ulong y0 = (uint)b;
            ulong y1 = b >> 32;

            ulong p11 = x1 * y1;
            ulong p01 = x0 * y1;
            ulong p10 = x1 * y0;
            ulong p00 = x0 * y0;

            ulong middle = p10 + (p00 >> 32) + (uint)p01;
            return p11 + (middle >> 32) + (p01 >> 32);
        }
#endif
    }
}