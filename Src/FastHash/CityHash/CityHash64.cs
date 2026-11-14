// C# port of google/cityhash 1.1.1 (4726e30). Copyright (c) 2011 Google, Inc.
// Distributed under the MIT license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Genbox.FastHash.CityHash.CityHashConstants;

namespace Genbox.FastHash.CityHash;

/// <summary>Provides 64-bit CityHash functions.</summary>
public static class CityHash64
{
    /// <summary>Computes the hash of a 64-bit value.</summary>
    /// <param name="input">The value to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        const ulong mul = K2 + 16;
        ulong a = input + K2;
        ulong b = input;
        ulong c = (RotateRight(b, 37) * mul) + a;
        ulong d = (RotateRight(a, 25) + b) * mul;
        return HashLen16(c, d, mul);
    }

    /// <summary>Computes the hash of data with the default seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => CityHash64Internal(data);

    /// <summary>Computes the hash of data with a seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed) => CityHash64WithSeeds(data, K2, seed);

    /// <summary>Computes the hash of data with two seeds.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed1">The first hash seed.</param>
    /// <param name="seed2">The second hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed1, ulong seed2) => CityHash64WithSeeds(data, seed1, seed2);

    private static ulong CityHash64Internal(ReadOnlySpan<byte> s)
    {
        ref byte sReference = ref MemoryMarshal.GetReference(s);
        uint len = (uint)s.Length;

        if (len <= 32)
        {
            if (len <= 16)
                return HashLen0to16(s, len);
            return HashLen17to32(s, len);
        }

        if (len <= 64)
            return HashLen33to64(s, len);

        // For strings over 64 bytes we hash the end first, and then as we
        // loop we keep 56 bytes of state: v, w, x, y, and z.
        ulong x = Read64(ref sReference, len - 40);
        ulong y = Read64(ref sReference, len - 16) + Read64(ref sReference, len - 56);
        ulong z = HashLen16(Read64(ref sReference, len - 48) + len, Read64(ref sReference, len - 24));
        UInt128 v = WeakHashLen32WithSeeds(s, len - 64, len, z);
        UInt128 w = WeakHashLen32WithSeeds(s, len - 32, y + K1, x);
        x = (x * K1) + Read64(ref sReference);

        // Decrease len to the nearest multiple of 64, and operate on 64-byte chunks.
        len = (len - 1) & ~63u;
        uint offset = 0;

        do
        {
            x = RotateRight(x + y + v.Low + Read64(ref sReference, offset + 8), 37) * K1;
            y = RotateRight(y + v.High + Read64(ref sReference, offset + 48), 42) * K1;
            x ^= w.High;
            y += v.Low + Read64(ref sReference, offset + 40);
            z = RotateRight(z + w.Low, 33) * K1;
            v = WeakHashLen32WithSeeds(s, offset, v.High * K1, x + w.Low);
            w = WeakHashLen32WithSeeds(s, offset + 32, z + w.High, y + Read64(ref sReference, offset + 16));
            Swap(ref z, ref x);
            offset += 64;
            len -= 64;
        } while (len != 0);

        return HashLen16(HashLen16(v.Low, w.Low) + (ShiftMix(y) * K1) + z, HashLen16(v.High, w.High) + x);
    }

    private static ulong CityHash64WithSeeds(ReadOnlySpan<byte> s, ulong seed0, ulong seed1) => HashLen16(CityHash64Internal(s) - seed0, seed1);

    // This probably works well for 16-byte strings as well, but it may be overkill in that case.
    private static ulong HashLen17to32(ReadOnlySpan<byte> s, uint len)
    {
        ref byte sReference = ref MemoryMarshal.GetReference(s);
        ulong mul = K2 + (len * 2);
        ulong a = Read64(ref sReference) * K1;
        ulong b = Read64(ref sReference, 8);
        ulong c = Read64(ref sReference, len - 8) * mul;
        ulong d = Read64(ref sReference, len - 16) * K2;
        return HashLen16(RotateRight(a + b, 43) + RotateRight(c, 30) + d, a + RotateRight(b + K2, 18) + c, mul);
    }

    // Return an 8-byte hash for 33 to 64 bytes.
    private static ulong HashLen33to64(ReadOnlySpan<byte> s, uint len)
    {
        ref byte sReference = ref MemoryMarshal.GetReference(s);
        ulong mul = K2 + (len * 2);
        ulong a = Read64(ref sReference) * K2;
        ulong b = Read64(ref sReference, 8);
        ulong c = Read64(ref sReference, len - 24);
        ulong d = Read64(ref sReference, len - 32);
        ulong e = Read64(ref sReference, 16) * K2;
        ulong f = Read64(ref sReference, 24) * 9;
        ulong g = Read64(ref sReference, len - 8);
        ulong h = Read64(ref sReference, len - 16) * mul;
        ulong u = RotateRight(a + g, 43) + ((RotateRight(b, 30) + c) * 9);
        ulong v = ((a + g) ^ d) + f + 1;
        ulong w = ByteSwap((u + v) * mul) + h;
        ulong x = RotateRight(e + f, 42) + c;
        ulong y = (ByteSwap((v + w) * mul) + g) * mul;
        ulong z = e + f + c;
        a = ByteSwap(((x + z) * mul) + y) + b;
        b = ShiftMix(((z + a) * mul) + d + h) * mul;
        return b + x;
    }

    internal static ulong HashLen0to16(ReadOnlySpan<byte> s, uint len)
    {
        ref byte sReference = ref MemoryMarshal.GetReference(s);
        if (len >= 8)
        {
            ulong mul = K2 + (len * 2);
            ulong a = Read64(ref sReference) + K2;
            ulong b = Read64(ref sReference, len - 8);
            ulong c = (RotateRight(b, 37) * mul) + a;
            ulong d = (RotateRight(a, 25) + b) * mul;
            return HashLen16(c, d, mul);
        }

        if (len >= 4)
        {
            ulong mul = K2 + (len * 2);
            ulong a = Read32(ref sReference);
            return HashLen16(len + (a << 3), Read32(ref sReference, len - 4), mul);
        }

        if (len > 0)
        {
            byte a = Unsafe.Add(ref sReference, 0);
            byte b = Unsafe.Add(ref sReference, (int)(len >> 1));
            byte c = Unsafe.Add(ref sReference, (int)(len - 1));
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
    internal static UInt128 WeakHashLen32WithSeeds(ReadOnlySpan<byte> s, uint offset, ulong a, ulong b) => WeakHashLen32WithSeeds(Read64(ref MemoryMarshal.GetReference(s), offset),
        Read64(ref MemoryMarshal.GetReference(s), offset + 8),
        Read64(ref MemoryMarshal.GetReference(s), offset + 16),
        Read64(ref MemoryMarshal.GetReference(s), offset + 24),
        a,
        b);

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
        return new UInt128(b + c, a + z);
    }

    internal static void Permute3<T>(ref T a, ref T b, ref T c)
    {
        Swap(ref a, ref b);
        Swap(ref a, ref c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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