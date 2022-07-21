using System.Numerics;
using System.Runtime.CompilerServices;

namespace Genbox.FastHash;

internal static class Utilities
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort Fetch16(byte[] p, uint offset = 0) => Unsafe.ReadUnaligned<ushort>(ref p[offset]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe ushort Fetch16(byte* ptr, int offset = 0) => *(ushort*)(ptr + offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Fetch32(byte[] p, uint offset = 0) => Unsafe.ReadUnaligned<uint>(ref p[offset]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe uint Fetch32(byte* ptr, int offset = 0) => *(uint*)(ptr + offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Fetch64(byte[] p, uint offset = 0) => Unsafe.ReadUnaligned<ulong>(ref p[offset]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe ulong Fetch64(byte* ptr, int offset = 0) => *(ulong*)(ptr + offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint RotateRightCheck(uint x, byte r) => r == 0 ? x : RotateRight(x, r);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong RotateRightCheck(ulong x, byte r) => r == 0 ? x : RotateRight(x, r);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint RotateRight(uint x, byte r) => BitOperations.RotateRight(x, r);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong RotateRight(ulong x, byte r) => BitOperations.RotateRight(x, r);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint RotateLeft(uint x, byte r) => BitOperations.RotateLeft(x, r);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong RotateLeft(ulong x, byte r) => BitOperations.RotateLeft(x, r);

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