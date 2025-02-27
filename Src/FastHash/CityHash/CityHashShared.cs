﻿using System.Runtime.CompilerServices;
using static Genbox.FastHash.CityHash.CityHashConstants;

namespace Genbox.FastHash.CityHash;

internal static class CityHashShared
{
    internal static ulong HashLen0to16(ReadOnlySpan<byte> s, uint len)
    {
        if (len >= 8)
        {
            ulong mul = K2 + (len * 2);
            ulong a = Read64(s) + K2;
            ulong b = Read64(s, len - 8);
            ulong c = (RotateRight(b, 37) * mul) + a;
            ulong d = (RotateRight(a, 25) + b) * mul;
            return HashLen16(c, d, mul);
        }
        if (len >= 4)
        {
            ulong mul = K2 + (len * 2);
            ulong a = Read32(s);
            return HashLen16(len + (a << 3), Read32(s, len - 4), mul);
        }
        if (len > 0)
        {
            byte a = s[0];
            byte b = s[(int)(len >> 1)];
            byte c = s[(int)(len - 1)];
            uint y = a + ((uint)b << 8);
            uint z = len + ((uint)c << 2);
            return ShiftMix((y * K2) ^ (z * K0)) * K2;
        }
        return K2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Mur(uint a, uint h)
    {
        // Helper from Murmur3 for combining two 32-bit values.

        //IQV: The rotate constants are different than murmur
        a *= C1;
        a = RotateRight(a, 17);
        a *= C2;
        h ^= a;
        h = RotateRight(h, 19);
        return (h * 5) + 0xe6546b64;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong ShiftMix(ulong val) => val ^ (val >> 47);

    // Return a 16-byte hash for s[0] ... s[31], a, and b.  Quick and dirty.
    internal static UInt128 WeakHashLen32WithSeeds(ReadOnlySpan<byte> s, uint offset, ulong a, ulong b)
    {
        return WeakHashLen32WithSeeds(Read64(s, offset),
            Read64(s, offset + 8),
            Read64(s, offset + 16),
            Read64(s, offset + 24),
            a,
            b);
    }

    // Return a 16-byte hash for 48 bytes.  Quick and dirty.
    // Callers do best to use "random-looking" values for a and b.
    internal static UInt128 WeakHashLen32WithSeeds(ulong w, ulong x, ulong y, ulong z, ulong a, ulong b)
    {
        a += w;
        b = RotateRight(b + a + z, 21);
        ulong c = a;
        a += x;
        a += y;
        b += RotateRight(a, 44);
        return new UInt128(a + z, b + c);
    }

    internal static void Permute3<T>(ref T a, ref T b, ref T c)
    {
        Swap(ref a, ref b);
        Swap(ref a, ref c);
    }

    internal static ulong HashLen16(ulong h1, ulong h2, ulong seed = 0x9ddfea08eb382d69UL)
    {
        // Murmur-inspired hashing.
        ulong a = (h1 ^ h2) * seed;
        a ^= a >> 47;
        ulong b = (h2 ^ a) * seed;
        b ^= b >> 47;
        b *= seed;
        return b;
    }
}