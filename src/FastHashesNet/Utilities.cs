using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Genbox.FastHashesNet;

public static class Utilities
{
    public static unsafe void ToULongs(byte[] data, out ulong a, out ulong b)
    {
        fixed (byte* ptr = data)
        {
            ulong* ulPtr = (ulong*)ptr;
            a = *ulPtr++;
            b = *ulPtr;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort Fetch16(byte[] p, int offset = 0)
    {
        return BitConverter.ToUInt16(p, offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe ushort Fetch16(byte* ptr, int offset = 0)
    {
        return *(ushort*)(ptr + offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Fetch32(byte[] p, int offset = 0)
    {
        return (uint)(p[0 + offset] | (p[1 + offset] << 8) | (p[2 + offset] << 16) | (p[3 + offset] << 24));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Fetch32(byte[] p, uint offset = 0)
    {
        return (uint)(p[0 + offset] | (p[1 + offset] << 8) | (p[2 + offset] << 16) | (p[3 + offset] << 24));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe uint Fetch32(byte* ptr, int offset = 0)
    {
        return *(uint*)(ptr + offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Fetch64(byte[] p, int offset = 0)
    {
        int i1 = p[0 + offset] | (p[1 + offset] << 8) | (p[2 + offset] << 16) | (p[3 + offset] << 24);

        int i2 = p[4 + offset] | (p[5 + offset] << 8) | (p[6 + offset] << 16) | (p[7 + offset] << 24);

        return (ulong)((uint)i1 | ((long)i2 << 32));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ulong Fetch64(byte* ptr, int offset = 0)
    {
        return *(ulong*)(ptr + offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint RotateWithCheck(uint x, int r)
    {
        // Avoid shifting by 32: doing so yields an undefined result.
        return r == 0 ? x : (x >> r) | (x << (32 - r));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong RotateWithCheck(ulong x, byte r)
    {
        // Avoid shifting by 64: doing so yields an undefined result.
        return r == 0 ? x : (x >> r) | (x << (64 - r));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Rotate(uint x, byte r)
    {
        return (x << r) | (x >> (32 - r));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Rotate(ulong x, byte r)
    {
        return (x << r) | (x >> (64 - r));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint FMix(uint h)
    {
        h ^= h >> 16;
        h *= 0x85ebca6b;
        h ^= h >> 13;
        h *= 0xc2b2ae35;
        h ^= h >> 16;
        return h;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong FMix(ulong h)
    {
        h ^= h >> 33;
        h *= 0xff51afd7ed558ccd;
        h ^= h >> 33;
        h *= 0xc4ceb9fe1a85ec53;
        h ^= h >> 33;
        return h;
    }
}