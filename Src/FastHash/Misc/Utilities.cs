using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET8_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif

namespace Genbox.FastHash.Misc;

internal static class Utilities
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort Read16(ReadOnlySpan<byte> data, int offset = 0) => Read16(ref MemoryMarshal.GetReference(data), offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort Read16(ref byte data, int offset = 0)
    {
        ushort value = Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref data, offset));
        return BitConverter.IsLittleEndian ? value : BinaryPrimitives.ReverseEndianness(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Read32(ReadOnlySpan<byte> data, int offset = 0) => Read32(ref MemoryMarshal.GetReference(data), offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Read32(ref byte data, int offset = 0)
    {
        uint value = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref data, offset));
        return BitConverter.IsLittleEndian ? value : BinaryPrimitives.ReverseEndianness(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Read32(ReadOnlySpan<byte> data, uint offset) => Read32(ref MemoryMarshal.GetReference(data), offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Read32(ref byte data, uint offset)
    {
        uint value = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref data, (IntPtr)offset));
        return BitConverter.IsLittleEndian ? value : BinaryPrimitives.ReverseEndianness(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Read64(ReadOnlySpan<byte> data, int offset = 0) => Read64(ref MemoryMarshal.GetReference(data), offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Read64(ref byte data, int offset = 0)
    {
        ulong value = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref data, offset));
        return BitConverter.IsLittleEndian ? value : BinaryPrimitives.ReverseEndianness(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Read64(ReadOnlySpan<byte> data, uint offset) => Read64(ref MemoryMarshal.GetReference(data), offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Read64(ref byte data, uint offset)
    {
        ulong value = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref data, (IntPtr)offset));
        return BitConverter.IsLittleEndian ? value : BinaryPrimitives.ReverseEndianness(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Write64(Span<byte> data, int offset, ulong value)
    {
        if (!BitConverter.IsLittleEndian)
            value = BinaryPrimitives.ReverseEndianness(value);

        ref byte ptr = ref MemoryMarshal.GetReference(data);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref ptr, offset), value);
    }

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
        return BigMulScalar(a, b, out low);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong BigMulScalar(ulong a, ulong b, out ulong low)
    {
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
    }


#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> ReadPartial128(ref byte data, int length)
    {
        // Callers supply 0-16 available bytes; never read beyond that extent.
        if (length == 16)
            return Unsafe.ReadUnaligned<Vector128<byte>>(ref data);

        if (!BitConverter.IsLittleEndian)
        {
            Vector128<byte> result = Vector128<byte>.Zero;
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<Vector128<byte>, byte>(ref result), ref data, (uint)length);
            return result;
        }
        if (length >= 8)
        {
            ulong low = Read64(ref data);
            ulong high = length == 8 ? 0 : Read64(ref data, length - 8) >> ((16 - length) * 8);
            return Vector128.Create(low, high).AsByte();
        }

        if (length >= 4)
        {
            uint low = Read32(ref data);
            uint high = length == 4 ? 0 : Read32(ref data, length - 4) >> ((8 - length) * 8);
            return Vector128.Create(low | ((ulong)high << 32), 0UL).AsByte();
        }

        uint value = 0;
        if (length != 0)
        {
            value = data;
            if (length >= 2)
                value |= (uint)Unsafe.Add(ref data, 1) << 8;
            if (length == 3)
                value |= (uint)Unsafe.Add(ref data, 2) << 16;
        }

        return Vector128.Create((ulong)value, 0UL).AsByte();
    }
#endif
}